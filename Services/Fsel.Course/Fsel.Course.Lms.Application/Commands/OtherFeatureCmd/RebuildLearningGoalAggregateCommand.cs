// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.OtherFeatureCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Models;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class RebuildLearningGoalAggregateCommand : IRequest<MethodResult<bool>>
    {
        public DateTime? ToDate { get; set; }
    }

    public class RebuildLearningGoalAggregateCommandHandler : IRequestHandler<RebuildLearningGoalAggregateCommand, MethodResult<bool>>
    {
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IStudentGoalAggregateRepository _aggregateRepo;
        private readonly IStudentGoalSummaryRepository _summaryRepo;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;
        private readonly ILearningGoalAggregateService _aggregateService;
        private readonly IStudentGoalProgressService _studentGoalProgressService;

        public RebuildLearningGoalAggregateCommandHandler(
            ICourseResultRepository courseResultRepository,
            IStudentGoalAggregateRepository aggregateRepo,
            IStudentGoalSummaryRepository summaryRepo,
            ISystemService systemService,
            IUserService userService,
            ILearningGoalAggregateService aggregateService,
            IStudentGoalProgressService studentGoalProgressService)
        {
            _courseResultRepository = courseResultRepository;
            _aggregateRepo = aggregateRepo;
            _summaryRepo = summaryRepo;
            _systemService = systemService;
            _userService = userService;
            _aggregateService = aggregateService;
            _studentGoalProgressService = studentGoalProgressService;
        }

        #region Handle

        public async Task<MethodResult<bool>> Handle(
            RebuildLearningGoalAggregateCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var referenceUtc = request.ToDate ?? DateTime.UtcNow;

            var (weekStartUtc, weekEndUtc) = DateTimeHelper.GetCurrentWeekRangeUtc(referenceUtc);
            var weekEndExclusiveUtc = weekEndUtc.Date.AddDays(1);

            var weekRangeExclusiveUtc = new DateRangeExclusive(weekStartUtc, weekEndExclusiveUtc);
            var weekRangeUtc = new DateRange(weekStartUtc, weekEndUtc);

            var courseGoals = await LoadCourseGoalsAsync(cancellationToken);

            await RebuildAggregatesAsync(courseGoals, weekRangeExclusiveUtc, weekRangeUtc, cancellationToken);
            await _studentGoalProgressService.RecalculateStudentGoalProgressAsync(referenceUtc, courseGoals, cancellationToken);

            return new MethodResult<bool> { Result = true };
        }

        #endregion Handle

        private async Task RebuildAggregatesAsync(
            IList<CourseGoalModel> courseGoals,
            DateRangeExclusive dateRangeExclusive,
            DateRange dateRange,
            CancellationToken ct)
        {
            await foreach (var batch in StreamStudentsAsync(ct))
            {
                var students = FilterStudentsWithCourse(batch);
                if (students.Count == 0)
                {
                    continue;
                }
                await CreateAggregatesForStudentsAsync(
                    students,
                    courseGoals,
                    dateRangeExclusive,
                    dateRange,
                    ct);
            }
        }

        private async IAsyncEnumerable<IList<StudentDetailModel>> StreamStudentsAsync(
            CancellationToken ct)
        {
            const int pageSize = 5000;
            var filters = new List<GenericFilterModel>
            {
                new() { Property = nameof(StudentModel.SchoolClassId), Operator = EnumFilterOperator.NotEmpty }
            };

            var page = 1;
            while (true)
            {
                var query = new BaseQueryModel
                {
                    Page = page,
                    PageSize = pageSize,
                    Filters = filters
                };

                var resp = await _userService.SearchAsync(query).ConfigureAwait(false);
                var items = resp?.Content?.Result?.Items;

                if (items == null || items.Count == 0)
                {
                    yield break;
                }
                yield return items;
                page++;
            }
        }

        private static List<StudentDetailModel> FilterStudentsWithCourse(
            IList<StudentDetailModel> students)
        {
            return students
                .Where(s => s.CourseId.HasValue && s.CourseId != Guid.Empty)
                .ToList();
        }

        private async Task CreateAggregatesForStudentsAsync(
            IList<StudentDetailModel> students,
            IList<CourseGoalModel> courseGoals,
            DateRangeExclusive dateRangeExclusive,
            DateRange dateRange,
            CancellationToken ct)
        {
            var (targets, courseResults) = await ResolveStudentsNeedingAggregateAsync(students, ct);
            if (targets.Count == 0)
            {
                return;
            }
            var pairs = ExtractStudentCoursePairs(targets);
            if (pairs.Count == 0)
            {
                return;
            }
            var totalLessons = await _aggregateService.LoadCourseLessonCountsAsync(pairs, ct);
            var stats = await _aggregateService.GetLessonCompletionStatsAsync(pairs, dateRangeExclusive, ct);

            var aggregates = BuildAggregates(
                courseResults,
                targets,
                courseGoals,
                totalLessons,
                stats);

            await PersistAggregatesAsync(
                aggregates,
                pairs,
                courseGoals,
                stats,
                dateRange,
                ct);
        }

        private async Task<(List<StudentDetailModel>, IList<CourseResult>)> ResolveStudentsNeedingAggregateAsync(
            IList<StudentDetailModel> students,
            CancellationToken ct)
        {
            var ids = students.Select(x => x.Id).ToList();

            var withoutAgg = await _aggregateService.GetCourseResultsWithoutAggregateAsync(ids, ct);

            var missing = withoutAgg.Select(x => x.StudentId).ToHashSet();
            var studentNotAgg = students.Where(s => missing.Contains(s.Id)).ToList();
            return (studentNotAgg, withoutAgg.ToList());
        }

        private static List<(Guid StudentId, Guid CourseId)> ExtractStudentCoursePairs(
            IEnumerable<StudentDetailModel> students)
        {
            return students.Select(s => (s.Id, s.CourseId!.Value)).ToList();
        }

        private static CourseGoalModel? ResolveCourseGoal(
        StudentDetailModel student,
        IDictionary<(Guid?, Guid?), List<CourseGoalModel>> lookup,
        Guid programId,
        Guid levelId)
        {
            if (!lookup.TryGetValue((programId, levelId), out var goals))
            {
                return null;
            }

            if (student.SchoolClassId.HasValue)
            {
                return goals.FirstOrDefault(x =>
                           x.GoalCategory == EnumCourseGoalCategory.School &&
                           x.ClassId == student.SchoolClassId)
                    ?? goals.FirstOrDefault(x => x.GoalCategory == EnumCourseGoalCategory.All);
            }

            return goals.FirstOrDefault(x =>
                       x.GoalCategory == EnumCourseGoalCategory.DefaultExcludeSchoolAndClass)
                ?? goals.FirstOrDefault(x => x.GoalCategory == EnumCourseGoalCategory.All);
        }

        private static CourseGoalConfigModel? ResolveCourseGoalConfig(CourseGoalModel goal, Guid courseId)
        {
            return goal.CourseGoalConfigs.FirstOrDefault(c => c.CourseId == courseId);
        }

        private static StudentGoalAggregate CreateAggregate(
        CourseResult courseResult,
        StudentDetailModel student,
        CourseGoalModel goal,
        CourseGoalConfigModel config,
        IDictionary<(Guid, Guid), int> totalLessons,
        IDictionary<(Guid, Guid), LessonCompletionStats> stats)
        {
            var key = (student.Id, student.CourseId!.Value);

            var total = totalLessons.TryGetValue(key, out var t) ? t : 0;
            var stat = stats.TryGetValue(key, out var s) ? s : default;

            return new StudentGoalAggregate
            {
                StudentId = student.Id,
                CourseResultId = courseResult.Id,
                ProgramId = courseResult.Course?.ProgramId,
                LevelId = courseResult.Course?.LevelId,

                CourseId = courseResult.CourseId,
                CourseGoalId = goal.Id,
                CourseGoalConfigId = config.Id,
                TotalTargetLessons = total,
                TotalCompletedLessons = stat.TotalCompleted,
                CombinedProgress = EnumCombinedProgress.TotalBehindWeekBehind,
                CurrentCombinedProgress = EnumCombinedProgress.TotalBehindWeekBehind,
                IsActive = true,
                SchoolId = student.SchoolId,
                SchoolName = student.School ?? goal.SchoolName,
                ClassId = student.SchoolClassId,
                ClassName = student.SchoolClass
            };
        }

        private static List<StudentGoalAggregate> BuildAggregates(
            IList<CourseResult> courseResults,
            IList<StudentDetailModel> students,
            IList<CourseGoalModel> courseGoals,
            IDictionary<(Guid, Guid), int> totalLessons,
            IDictionary<(Guid, Guid), LessonCompletionStats> stats)
        {
            var studentMap = students.ToDictionary(s => s.Id);
            var aggregates = new List<StudentGoalAggregate>();

            var goalLookup = courseGoals
                .GroupBy(g => (g.ProgramId, g.LevelId))
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var courseResult in courseResults)
            {
                var student = studentMap[courseResult.StudentId];
                if (!student.CourseId.HasValue)
                {
                    continue;
                }
                if (courseResult.Course == null)
                {
                    continue;
                }

                var courseGoal = ResolveCourseGoal(student, goalLookup, courseResult.Course.ProgramId ?? Guid.Empty, courseResult.Course.LevelId ?? Guid.Empty);
                if (courseGoal == null)
                {
                    continue;
                }
                var config = ResolveCourseGoalConfig(courseGoal, student.CourseId.Value);
                if (config == null)
                {
                    continue;
                }
                var aggregate = CreateAggregate(
                    courseResult,
                    student,
                    courseGoal,
                    config,
                    totalLessons,
                    stats);

                aggregates.Add(aggregate);
            }

            return aggregates;
        }

        private async Task PersistAggregatesAsync(
            List<StudentGoalAggregate> aggregates,
            List<(Guid StudentId, Guid CourseId)> pairs,
            IList<CourseGoalModel> goals,
            IDictionary<(Guid, Guid), LessonCompletionStats> stats,
            DateRange weekRange,
            CancellationToken ct)
        {
            if (aggregates.Count == 0)
            {
                return;
            }
            var existing = await LoadExistingAggregateKeysAsync(pairs, ct);

            var toInsert = aggregates
                .Where(a => !existing.Contains((a.StudentId, a.CourseId, a.CourseResultId)))
                .ToList();

            if (toInsert.Count == 0)
            {
                return;
            }
            await _aggregateRepo.AddList(toInsert);
            await _aggregateRepo.UnitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

            await EnsureWeeklySummariesAsync(toInsert, goals, weekRange, stats, ct);
        }

        private async Task<HashSet<(Guid, Guid, Guid)>> LoadExistingAggregateKeysAsync(
            List<(Guid StudentId, Guid CourseId)> pairs,
            CancellationToken ct)
        {
            var studentCourseKeys = pairs.Select(x => new { x.StudentId, x.CourseId }).ToList();

            var agg = _aggregateRepo.Queryable.WhereBulkContains(studentCourseKeys, new[] { "StudentId", "CourseId" });
            var query = from baseQ in agg
                        join cr in _courseResultRepository.Queryable on baseQ.CourseResultId equals cr.Id
                        select new
                        {
                            StudentId = cr.StudentId,
                            CourseId = cr.CourseId,
                            CourseResultId = cr.Id
                        };

            var rows = await query.ToListAsync(ct);

            return rows.Select(x => (x.StudentId, x.CourseId, x.CourseResultId)).ToHashSet();
        }

        private async Task EnsureWeeklySummariesAsync(
        IList<StudentGoalAggregate> aggregates,
        IList<CourseGoalModel> goals,
        DateRange weekRange,
        IDictionary<(Guid, Guid), LessonCompletionStats> stats,
        CancellationToken ct)
        {
            var goalById = goals.ToDictionary(g => g.Id);
            var summaries = new List<StudentGoalSummary>();

            foreach (var ag in aggregates)
            {
                if (!goalById.TryGetValue(ag.CourseGoalId, out var goal))
                {
                    continue;
                }
                var cfg = goal.CourseGoalConfigs.FirstOrDefault(x => x.Id == ag.CourseGoalConfigId);
                if (cfg == null)
                {
                    continue;
                }
                var stat = stats.TryGetValue((ag.StudentId, ag.CourseId), out var s) ? s : default;

                summaries.Add(new StudentGoalSummary
                {
                    StudentGoalAggregateId = ag.Id,
                    StartDate = weekRange.WeekStart,
                    EndDate = weekRange.WeekEnd,

                    CompletedLessons = stat.CompletedThisWeek,
                    LessonsPerWeek = cfg.LessonsPerWeek,
                    TotalCompletedLessons = ag.TotalCompletedLessons,
                    TotalTargetLessons = ag.TotalTargetLessons,
                    LastCompletedAt = stat.LastCompletedAt,
                    ProgressStatus = EnumCombinedProgressHelper.GetProgressStatusFromCounts(stat.CompletedThisWeek, cfg.LessonsPerWeek)
                });
            }

            if (summaries.Count == 0)
            {
                return;
            }
            await _summaryRepo.AddList(summaries);
            await _summaryRepo.UnitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
        }

        private async Task<IList<CourseGoalModel>> LoadCourseGoalsAsync(CancellationToken ct)
        {
            var res = await _systemService.GetListCourseGoalAsync();
            return res?.Content?.Result ?? new List<CourseGoalModel>();
        }
    }
}

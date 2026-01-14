// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

    public interface IStudentGoalProgressService
    {
        Task RecalculateStudentGoalProgressAsync(
            DateTime referenceUtc,
            IList<CourseGoalModel> allCourseGoals,
            CancellationToken cancellationToken);
    }

    public sealed class StudentGoalProgressService : IStudentGoalProgressService
    {
        private readonly IStudentGoalAggregateRepository _aggregateRepository;
        private readonly ICourseResultRepository _resultRepository;
        private readonly IUserService _userService;
        private readonly IStudentGoalSummaryRepository _summaryRepository;
        private readonly ILessonResultRepository _lessonResultRepository;

        public StudentGoalProgressService(
            IStudentGoalAggregateRepository aggregateRepository,
            ICourseResultRepository resultRepository,
            IUserService userService,
            IStudentGoalSummaryRepository summaryRepository,
            ILessonResultRepository lessonResultRepository)
        {
            _aggregateRepository = aggregateRepository;
            _resultRepository = resultRepository;
            _userService = userService;
            _summaryRepository = summaryRepository;
            _lessonResultRepository = lessonResultRepository;
        }

        #region Orchestration

        public async Task RecalculateStudentGoalProgressAsync(DateTime referenceUtc,
            IList<CourseGoalModel> allCourseGoals,
            CancellationToken cancellationToken)
        {
            var weekRange = GetCurrentWeek(referenceUtc);

            var aggregates = await LoadWorkingAggregatesAsync(cancellationToken);
            if (aggregates.Count == 0)
            {
                return;
            }

            var studentMap = await LoadStudentsAsync(aggregates);

            await EnsureCurrentWeekSummariesAsync(
            aggregates,
            weekRange,
            allCourseGoals,
            cancellationToken);

            var latestWeeklyMap = LoadLatestWeeklySummaries(aggregates, weekRange);
            var weeklyTotalsMap = LoadWeeklyTotals(aggregates, weekRange);
            var lessonStatsMap = await LoadDoneLessonStatsCourseAsync(weekRange, cancellationToken);

            var summariesToUpdate = new List<StudentGoalSummary>();

            foreach (var aggregate in aggregates)
            {
                ProcessAggregate(
                    aggregate,
                    referenceUtc,
                    studentMap,
                    latestWeeklyMap,
                    weeklyTotalsMap,
                    lessonStatsMap,
                    summariesToUpdate);
            }

            _summaryRepository.UpdateList(summariesToUpdate);
            await _summaryRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            _aggregateRepository.UpdateList(aggregates);
            await _aggregateRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            await EnsureCurrentWeekSummariesAsync(aggregates.Where(x => x.IsActive).ToList(), weekRange, allCourseGoals, cancellationToken);
        }

        #endregion Orchestration

        #region Ensure Weekly Summary

        private async Task EnsureCurrentWeekSummariesAsync(
            IReadOnlyCollection<StudentGoalAggregate> aggregates,
            DateRange weekRange,
            IList<CourseGoalModel> allCourseGoals,
            CancellationToken ct)
        {
            var courseGoalById = allCourseGoals.ToDictionary(x => x.Id);
            var toInsert = new List<StudentGoalSummary>();

            foreach (var agg in aggregates.Where(x => x.IsActive))
            {
                var hasThisWeek = agg.StudentGoalSummaries?.Any(s =>
                    s.StartDate >= weekRange.WeekStart &&
                    s.EndDate <= weekRange.WeekEnd) == true;

                if (hasThisWeek)
                {
                    continue;
                }

                if (!courseGoalById.TryGetValue(agg.CourseGoalId, out var courseGoal))
                {
                    continue;
                }

                var cfg = courseGoal.CourseGoalConfigs
                                    ?.FirstOrDefault(c => c.Id == agg.CourseGoalConfigId);
                if (cfg == null)
                {
                    continue;
                }

                toInsert.Add(new StudentGoalSummary
                {
                    StudentGoalAggregateId = agg.Id,
                    StartDate = weekRange.WeekStart,
                    EndDate = weekRange.WeekEnd,
                    LessonsPerWeek = cfg.LessonsPerWeek,
                    TotalTargetLessons = agg.TotalTargetLessons,
                    TotalCompletedLessons = agg.TotalCompletedLessons,
                    ProgressStatus = EnumProgressStatus.Behind
                });
            }

            if (toInsert.Count > 0)
            {
                await _summaryRepository.AddList(toInsert);
                await _summaryRepository.UnitOfWork.SaveChangesAsync(ct);
            }
        }

        #endregion Ensure Weekly Summary

        #region Aggregate Processing

        private static void ProcessAggregate(
            StudentGoalAggregate aggregate,
            DateTime referenceUtc,
            IReadOnlyDictionary<Guid, StudentModel> studentMap,
            IReadOnlyDictionary<Guid, StudentGoalSummary> latestWeeklyMap,
            IReadOnlyDictionary<Guid, (int totalDone, int totalPlan, List<EnumProgressStatus> statuses)> weeklyTotalsMap,
            IReadOnlyDictionary<(Guid studentId, Guid courseId, Guid? courseResultId),
                                 (int total, int week, DateTime? last)> lessonStatsMap,
            IList<StudentGoalSummary> summariesToUpdate)
        {
            if (!TryResolveContext(
                aggregate,
                studentMap,
                latestWeeklyMap,
                lessonStatsMap,
                out var context))
            {
                return;
            }

            UpdateActiveState(aggregate, context.Student);

            if (!aggregate.IsActive)
            {
                return;
            }

            UpdateWeeklySummary(context.Weekly, context.LessonStats);
            summariesToUpdate.Add(context.Weekly);

            UpdateAggregateProgress(
                aggregate,
                context.Weekly,
                weeklyTotalsMap,
                referenceUtc);
        }

        #endregion Aggregate Processing

        #region Domain Logic

        private static void UpdateActiveState(StudentGoalAggregate aggregate, StudentModel student)
        {
            aggregate.IsActive =
                aggregate.StudentId == student.Id &&
                aggregate.CourseId == student.CourseId;
        }

        private static void UpdateWeeklySummary(
            StudentGoalSummary weekly,
            (int total, int week, DateTime? last) stats)
        {
            weekly.CompletedLessons = stats.week;
            weekly.LastCompletedAt = stats.last;
            weekly.TotalCompletedLessons = stats.total;
            weekly.ProgressStatus =
                EnumCombinedProgressHelper.GetProgressStatusFromCounts(
                    weekly.CompletedLessons,
                    weekly.LessonsPerWeek);
        }

        private static void UpdateAggregateProgress(
            StudentGoalAggregate aggregate,
            StudentGoalSummary weekly,
            IReadOnlyDictionary<Guid, (int totalDone, int totalPlan, List<EnumProgressStatus> statuses)> weeklyTotalsMap,
            DateTime referenceUtc)
        {
            var totals = weeklyTotalsMap.TryGetValue(aggregate.Id, out var t)
                ? t
                : (0, 0, new List<EnumProgressStatus>());

            var currentProgress =
                EnumCombinedProgressHelper.GetCurrentCombineProgress(
                    totals.Item1,
                    totals.Item2,
                    weekly.ProgressStatus);

            aggregate.CurrentCombinedProgress = currentProgress;

            aggregate.CombinedProgress =
                weekly.StartDate <= referenceUtc && weekly.EndDate >= referenceUtc
                    ? currentProgress
                    : EnumCombinedProgressHelper.GetCombineProgress(
                        totals.Item1,
                        totals.Item2);

            UpdateBehindStreak(aggregate, totals.Item3);
        }

        private static void UpdateBehindStreak(
            StudentGoalAggregate agg,
            IReadOnlyList<EnumProgressStatus> orderedStatuses)
        {
            var streak = 0;

            for (var i = orderedStatuses.Count - 1; i >= 0; i--)
            {
                if (orderedStatuses[i] == EnumProgressStatus.Behind)
                {
                    streak++;
                }
                else
                {
                    break;
                }
            }

            agg.ConsecutiveBehindWeeks = streak;
        }

        #endregion Domain Logic

        #region Context Resolution

        private static bool TryResolveContext(
            StudentGoalAggregate aggregate,
            IReadOnlyDictionary<Guid, StudentModel> studentMap,
            IReadOnlyDictionary<Guid, StudentGoalSummary> latestWeeklyMap,
            IReadOnlyDictionary<(Guid, Guid, Guid?), (int total, int week, DateTime? last)> lessonStatsMap,
            out AggregateContext context)
        {
            context = default;

            if (!studentMap.TryGetValue(aggregate.StudentId, out var student))
            {
                return false;
            }

            if (!latestWeeklyMap.TryGetValue(aggregate.Id, out var weekly))
            {
                return false;
            }

            lessonStatsMap.TryGetValue(
                (aggregate.StudentId, aggregate.CourseId, aggregate.CourseResultId),
                out var lessonStats);

            context = new AggregateContext(student, weekly, lessonStats);
            return true;
        }

        private sealed record AggregateContext(
            StudentModel Student,
            StudentGoalSummary Weekly,
            (int total, int week, DateTime? last) LessonStats);

        #endregion Context Resolution

        #region Data Loading

        private static DateRange GetCurrentWeek(DateTime referenceUtc)
        {
            var (start, end) = DateTimeHelper.GetCurrentWeekRangeNow(referenceUtc);
            return new DateRange(start, end);
        }

        private async Task<List<StudentGoalAggregate>> LoadWorkingAggregatesAsync(CancellationToken ct)
        {
            return await QueryWorkingStudentGoalAggregates()
                .Include(x => x.StudentGoalSummaries)
                .ToListAsync(ct);
        }

        private async Task<Dictionary<Guid, StudentModel>> LoadStudentsAsync(IReadOnlyCollection<StudentGoalAggregate> aggregates)
        {
            var studentIds = aggregates.Select(x => x.StudentId).Distinct().ToList();
            var result = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            return (result.Content?.Result ?? new List<StudentModel>())
                .ToDictionary(x => x.Id);
        }

        private IQueryable<StudentGoalAggregate> QueryWorkingStudentGoalAggregates()
        {
            var aggQuery = _aggregateRepository.Queryable
                .Where(x => x.TotalCompletedLessons != x.TotalTargetLessons);

            return from agg in aggQuery
                   join cr in _resultRepository.Queryable
                       on agg.CourseResultId equals cr.Id
                   where cr.WorkingStatus != EnumWorkingStatus.NotWorking
                   select agg;
        }

        private static Dictionary<Guid, StudentGoalSummary> LoadLatestWeeklySummaries(
            IReadOnlyCollection<StudentGoalAggregate> aggregates,
            DateRange weekRange)
        {
            return aggregates.ToDictionary(
                x => x.Id,
                x => x.StudentGoalSummaries
                      .Where(s => s.EndDate.Date <= weekRange.WeekEnd)
                      .OrderByDescending(s => s.EndDate)
                      .First());
        }

        private static Dictionary<Guid, (int, int, List<EnumProgressStatus>)> LoadWeeklyTotals(
            IReadOnlyCollection<StudentGoalAggregate> aggregates,
            DateRange weekRange)
        {
            return aggregates.Select(agg =>
            {
                var summaries = agg.StudentGoalSummaries
                    .Where(s => s.EndDate < weekRange.WeekEnd)
                    .OrderBy(s => s.StartDate)
                    .ToList();

                return new
                {
                    agg.Id,
                    TotalDone = summaries.Sum(x => x.CompletedLessons),
                    TotalPlan = summaries.Sum(x => x.LessonsPerWeek),
                    Statuses = summaries.Select(x => x.ProgressStatus).ToList()
                };
            }).ToDictionary(
                x => x.Id,
                x => (x.TotalDone, x.TotalPlan, x.Statuses));
        }

        private async Task<Dictionary<(Guid, Guid, Guid?), (int total, int week, DateTime? last)>> LoadDoneLessonStatsCourseAsync(
            DateRange dateRange,
            CancellationToken ct)
        {
            var rows = await (
                from cr in QueryWorkingCourseResults()
                join lr in _lessonResultRepository.ReadQueryable
                    on cr.Id equals lr.CourseResultId
                where lr.Status == EnumResultStatus.Done && (lr.CompletionDate ?? lr.UpdatedDate ?? lr.CreatedDate).Date <= dateRange.WeekEnd
                select new
                {
                    lr.StudentId,
                    lr.CourseId,
                    lr.CourseResultId,
                    CompletedAt = lr.CompletionDate ?? lr.UpdatedDate
                }).ToListAsync(ct);

            return rows.GroupBy(x => new { x.StudentId, x.CourseId, x.CourseResultId })
                       .ToDictionary(
                           g => (g.Key.StudentId, g.Key.CourseId, g.Key.CourseResultId),
                           g => (
                               total: g.Count(),
                               week: g.Count(x => x.CompletedAt >= dateRange.WeekStart &&
                                                   x.CompletedAt < dateRange.WeekEnd),
                               last: g.Max(x => x.CompletedAt)
                           ));
        }

        private IQueryable<CourseResult> QueryWorkingCourseResults()
        {
            return from agg in _aggregateRepository.ReadQueryable
                   join cr in _resultRepository.ReadQueryable
                       on agg.CourseResultId equals cr.Id
                   where agg.TotalCompletedLessons != agg.TotalTargetLessons
                      && cr.WorkingStatus != EnumWorkingStatus.NotWorking
                   select cr;
        }

        #endregion Data Loading
    }
}

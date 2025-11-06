// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.OtherFeatureCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
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
    }

    public class RebuildLearningGoalAggregateCommandHandler : IRequestHandler<RebuildLearningGoalAggregateCommand, MethodResult<bool>>
    {
        private readonly IStudentGoalAggregateRepository _studentLearningGoalAggregateRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly IStudentGoalSummaryRepository _studentLearningGoalSummaryRepository;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;

        public RebuildLearningGoalAggregateCommandHandler(
            IStudentGoalAggregateRepository studentLearningGoalAggregateRepository,
            ICourseResultRepository courseResultRepository,
            ILessonResultRepository lessonResultRepository,
            ICourseUnitMockTestRepository courseUnitMockTestRepository,
            IStudentGoalSummaryRepository studentLearningGoalSummaryRepository,
            ISystemService systemService,
            IUserService userService)
        {
            _studentLearningGoalAggregateRepository = studentLearningGoalAggregateRepository;
            _courseResultRepository = courseResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _studentLearningGoalSummaryRepository = studentLearningGoalSummaryRepository;
            _systemService = systemService;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(RebuildLearningGoalAggregateCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<bool>();

            // 0) CourseGoals
            var courseGoals = await GetCourseGoalsAsync(cancellationToken).ConfigureAwait(false);

            // 1-2) Stream học viên theo trang → tạo Aggregate cho từng batch (giảm RAM)
            await foreach (var studentBatch in StreamStudentsByPageAsync(cancellationToken))
            {
                var studentsWithCourse = studentBatch
                    .Where(s => s.CourseId.HasValue && s.CourseId != Guid.Empty)
                    .ToList();
                if (studentsWithCourse.Count == 0)
                {
                    continue;
                }
                await CreateStudentAggregateAsync(studentsWithCourse, courseGoals, cancellationToken).ConfigureAwait(false);

                // Clear tracking giữa các batch
                // No direct DbContext available from UnitOfWork; skip explicit ChangeTracker.Clear()
                // Rely on: AsNoTracking for reads + chunked saves for writes to keep tracker small.
            }

            // 3) Tái xây dựng tiến độ học tập (dùng số liệu gộp, no-tracking)
            await RebuildStudentLearningProgressAsync(courseGoals, cancellationToken).ConfigureAwait(false);

            methodResult.Result = true;
            return methodResult;
        }

        // =========================
        // Data loaders (memory-safe)
        // =========================

        private async Task<IList<CourseGoalModel>> GetCourseGoalsAsync(CancellationToken ct)
        {
            var courseGoalRes = await _systemService.GetListCourseGoalAsync().ConfigureAwait(false);
            return courseGoalRes?.Content?.Result ?? new List<CourseGoalModel>(0);
        }

        private async IAsyncEnumerable<IList<StudentDetailModel>> StreamStudentsByPageAsync(
            [EnumeratorCancellation] CancellationToken ct)
        {
            const int pageSize = 5000; // tune theo RAM/DB
            var baseFilter = new List<GenericFilterModel>
            {
                new() { Property = nameof(StudentModel.SchoolClassId), Operator = EnumFilterOperator.NotEmpty }
            };

            var page = 1;
            while (true)
            {
                var q = new BaseQueryModel { Page = page, PageSize = pageSize, Filters = baseFilter };
                var resp = await _userService.SearchAsync(q).ConfigureAwait(false);
                var result = resp?.Content?.Result;
                var items = result?.Items;

                if (items == null || items.Count == 0)
                {
                    yield break;
                }
                yield return items;

                page++;
                var totalPages = result?.PagingInfo?.TotalItems / pageSize ?? page;
                if (page > totalPages)
                {
                    yield break;
                }
            }
        }

        private async Task<IList<Guid>> GetStudentIdsWithCourseResultAsync(IList<Guid> studentIds, CancellationToken ct = default)
        {
            return await _courseResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId)
                .AsNoTracking()
                .Select(x => x.StudentId)
                .Distinct()
                .ToListAsync(ct);
        }

        private async Task<IList<CourseResult>> GetUnlinkedCourseResultsAsync(IList<Guid> studentIds, CancellationToken ct = default)
        {
            var query = from cr in _courseResultRepository.Queryable.Include(x => x.Course).WhereBulkContains(studentIds, x => x.StudentId)
                        where cr.WorkingStatus != EnumWorkingStatus.NotWorking && cr.Status != EnumResultStatus.Done
                        join ag0 in _studentLearningGoalAggregateRepository.Queryable.AsNoTracking().Where(x => x.IsActive)
                            on new { cr.CourseId, cr.StudentId }
                            equals new { ag0.CourseId, ag0.StudentId } into agGroup
                        from ag in agGroup.DefaultIfEmpty()
                        where ag == null
                        select cr;

            return await query.AsNoTracking().ToListAsync(ct);
        }

        // Đếm lesson của khóa học trực tiếp từ bảng UnitLesson (tránh nạp navigation)
        private async Task<Dictionary<Guid, int>> LoadCourseLessonCountsAsync(IEnumerable<Guid> courseIds, CancellationToken ct)
        {
            var rows = await _courseUnitMockTestRepository.Queryable.AsNoTracking()
                                                          .WhereBulkContains(courseIds, x => x.CourseId)
                                                          .Select(x => new { x.CourseId, Count = x.Unit!.UnitLessons.Count })
                                                          .ToListAsync(ct);
            return rows.GroupBy(x => x.CourseId).ToDictionary(g => g.Key, g => g.Sum(x => x.Count));
        }

        // Lấy số liệu gộp LessonResult: total, week, last
        private async Task<Dictionary<(Guid StudentId, Guid CourseId), (int total, int week, DateTime? last)>>
            LoadDoneLessonStatsAsync(IList<(Guid StudentId, Guid CourseId)> keys, CancellationToken ct)
        {
            if (keys == null || keys.Count == 0)
                return new();

            var (weekStart, weekEnd) = Shared.Helpers.DateTimeHelper.GetCurrentWeekRangeNow();
            var keyDtos = keys.Distinct().Select(k => new { k.StudentId, k.CourseId }).ToList();

            var rows = await _lessonResultRepository.Queryable
                .AsNoTracking()
                .Where(x => x.Status == EnumResultStatus.Done)
                .WhereBulkContains(keyDtos, new[] { "StudentId", "CourseId" })
                .Select(x => new
                {
                    x.StudentId,
                    x.CourseId,
                    CompletedAt = x.CompletionDate ?? x.UpdatedDate,
                    IsInWeek = (x.CompletionDate ?? x.UpdatedDate) >= weekStart && (x.CompletionDate ?? x.UpdatedDate) <= weekEnd
                })
                .GroupBy(g => new { g.StudentId, g.CourseId })
                .Select(g => new
                {
                    g.Key.StudentId,
                    g.Key.CourseId,
                    Total = g.Count(),
                    Week = g.Count(r => r.IsInWeek),
                    Last = g.Max(r => r.CompletedAt)
                })
                .ToListAsync(ct)
                .ConfigureAwait(false);

            return rows.ToDictionary(
                k => (k.StudentId, k.CourseId),
                v => (v.Total, v.Week, v.Last));
        }

        // Weekly mới nhất mỗi Aggregate (no-tracking)
        private async Task<Dictionary<Guid, StudentGoalSummary>> FetchLatestWeeklyByAggregateIdAsync(CancellationToken ct)
        {
            var latest = await _studentLearningGoalSummaryRepository.Queryable
                .AsNoTracking()
                .GroupBy(s => s.StudentGoalAggregateId)
                .Select(g => g.OrderByDescending(x => x.StartDate).First())
                .ToListAsync(ct)
                .ConfigureAwait(false);

            return latest.ToDictionary(x => x.StudentGoalAggregateId, x => x);
        }

        // Tổng weekly (<= today) theo AggregateId
        private async Task<Dictionary<Guid, (int totalDone, int totalPlan)>> LoadWeeklyTotalsAsync(
            IEnumerable<Guid> aggregateIds, DateTime today, CancellationToken ct)
        {
            var ids = aggregateIds.Distinct().ToList();
            if (ids.Count == 0)
                return new();

            var rows = await _studentLearningGoalSummaryRepository.Queryable
                .AsNoTracking()
                .Where(x => ids.Contains(x.StudentGoalAggregateId) && x.EndDate <= today)
                .GroupBy(x => x.StudentGoalAggregateId)
                .Select(g => new
                {
                    AggregateId = g.Key,
                    TotalDone = g.Sum(s => s.CompletedLessons),
                    TotalPlan = g.Sum(s => s.LessonsPerWeek)
                })
                .ToListAsync(ct)
                .ConfigureAwait(false);

            return rows.ToDictionary(k => k.AggregateId, v => (v.TotalDone, v.TotalPlan));
        }

        // =========================
        // Build/Ensure helpers
        // =========================

        private async Task CreateStudentAggregateAsync(IList<StudentDetailModel> students, IList<CourseGoalModel> courseGoals, CancellationToken ct)
        {
            var studentIds = students.Select(x => x.Id).ToList();
            var unlinkedCourseResults = await GetUnlinkedCourseResultsAsync(students.Select(x => x.Id).ToList(), ct).ConfigureAwait(false);
            var unlinkedStudentIds = unlinkedCourseResults
                .Select(x => x.StudentId)
                .Distinct()
                .ToHashSet();

            var studentIdsWithCourseResult = await GetStudentIdsWithCourseResultAsync(studentIds, ct).ConfigureAwait(false);
            var studentsWithoutCourse = students
                .Where(s => !studentIdsWithCourseResult.Contains(s.Id) || unlinkedStudentIds.Contains(s.Id))
                .ToList();

            var courseIds = studentsWithoutCourse
                .Where(x => x.CourseId.HasValue)
                .Select(s => s.CourseId!.Value)
                .Distinct()
                .ToList();

            var totalLessonsPerCourse = await LoadCourseLessonCountsAsync(courseIds, ct).ConfigureAwait(false);

            var courseResults = studentsWithoutCourse
                .Where(x => x.CourseId.HasValue && x.CourseId != Guid.Empty)
                .Select(s => new CourseResultModel { StudentId = s.Id, CourseId = s.CourseId ?? default })
                .ToList();

            var studentCourseKeys = studentsWithoutCourse
                .Where(x => x.CourseId.HasValue && x.CourseId != Guid.Empty)
                .Select(s => new { StudentId = s.Id, CourseId = s.CourseId ?? default })
                .ToList();

            var existingKeys = await _studentLearningGoalAggregateRepository.Queryable
                .AsNoTracking()
                .WhereBulkContains(studentCourseKeys, new[] { "StudentId", "CourseId" })
                .Select(a => new { a.StudentId, a.CourseId })
                .ToListAsync(ct)
                .ConfigureAwait(false);

            var pairs = courseResults.Select(s => (s.StudentId, s.CourseId)).Distinct().ToList();
            var doneStats = await LoadDoneLessonStatsAsync(pairs, ct).ConfigureAwait(false);

            var aggregatesToInsert = BuildAggregates(courseResults, courseGoals, students, totalLessonsPerCourse, doneStats);
            if (aggregatesToInsert.Count == 0)
            {
                return;
            }
            var existingKeySet = existingKeys.Select(k => (k.StudentId, k.CourseId)).ToHashSet();
            aggregatesToInsert = aggregatesToInsert
                .Where(a => !existingKeySet.Contains((a.StudentId, a.CourseId)))
                .ToList();

            if (aggregatesToInsert.Any())
            {
                // No direct DbContext from UnitOfWork → save in chunks
                const int batch = 2000;
                for (int i = 0; i < aggregatesToInsert.Count; i += batch)
                {
                    var slice = aggregatesToInsert.Skip(i).Take(batch).ToList();
                    await _studentLearningGoalAggregateRepository.AddList(slice);
                    await _studentLearningGoalAggregateRepository.UnitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
                }

                await EnsureWeeklySummariesForAggregatesAsync(aggregatesToInsert, courseGoals, doneStats, ct).ConfigureAwait(false);
            }
        }

        private static List<StudentGoalAggregate> BuildAggregates(
            IEnumerable<CourseResultModel> courseResults,
            IList<CourseGoalModel> courseGoals,
            IList<StudentDetailModel> students,
            IDictionary<Guid, int> totalLessonsPerCourse,
            IDictionary<(Guid StudentId, Guid CourseId), (int total, int week, DateTime? last)> doneStats)
        {
            var aggregates = new List<StudentGoalAggregate>();
            var studentMap = students.ToDictionary(s => s.Id);

            var cgLookup = courseGoals
                .GroupBy(g => (g.CourseLevel, g.CourseType))
                .ToDictionary(k => k.Key, v => v.ToList());

            foreach (var cr in courseResults)
            {
                if (!studentMap.TryGetValue(cr.StudentId, out var student))
                {
                    continue;
                }
                var level = cr.CourseLevel ?? student.CourseLevel ?? default;
                var type = level.GetEnumCourseType();

                if (!cgLookup.TryGetValue((level, type), out var list))
                {
                    continue;
                }
                CourseGoalModel? courseGoal;
                if (student.SchoolClassId.HasValue)
                {
                    courseGoal = list.FirstOrDefault(x => x.GoalCategory == EnumCourseGoalCategory.School && x.ClassId == student.SchoolClassId.Value)
                               ?? list.FirstOrDefault(x => x.GoalCategory == EnumCourseGoalCategory.All);
                }
                else
                {
                    courseGoal = list.FirstOrDefault(x => x.GoalCategory == EnumCourseGoalCategory.DefaultExcludeSchoolAndClass)
                               ?? list.FirstOrDefault(x => x.GoalCategory == EnumCourseGoalCategory.All);
                }
                var courseGoalConfig = courseGoal?.CourseGoalConfigs.FirstOrDefault(x => x.CourseId == cr.CourseId);
                if (courseGoal == null || courseGoalConfig == null)
                {
                    continue;
                }
                var totalTargetLessons = totalLessonsPerCourse.TryGetValue(cr.CourseId, out var total) ? total : 0;
                var stat = doneStats.TryGetValue((cr.StudentId, cr.CourseId), out var s) ? s : default;

                var ag = new StudentGoalAggregate
                {
                    CombinedProgress = EnumCombinedProgress.TotalBehindWeekBehind,
                    CurrentCombinedProgress = EnumCombinedProgress.TotalBehindWeekBehind,
                    StudentId = cr.StudentId,
                    CourseId = cr.CourseId,
                    CourseGoalId = courseGoal.Id,
                    TotalCompletedLessons = stat.total,
                    CourseGoalConfigId = courseGoalConfig.Id,
                    TotalTargetLessons = totalTargetLessons,
                    CourseLevel = level,
                    IsActive = true,
                    CourseType = type,
                };
                if (student.SchoolClassId.HasValue)
                {
                    ag.SchoolId = student.SchoolId;
                    ag.SchoolName = student.School ?? courseGoal.SchoolName;
                    ag.ClassId = student.SchoolClassId;
                    ag.ClassName = student.SchoolClass;
                }
                aggregates.Add(ag);
            }

            return aggregates;
        }

        private async Task EnsureWeeklySummariesForAggregatesAsync(
            IList<StudentGoalAggregate> aggregates,
            IList<CourseGoalModel> allCourseGoals,
            IDictionary<(Guid StudentId, Guid CourseId), (int total, int week, DateTime? last)> doneStats,
            CancellationToken ct)
        {
            if (aggregates.Count == 0)
            {
                return;
            }
            var (weekStartUtc, weekEndUtc) = Shared.Helpers.DateTimeHelper.GetCurrentWeekRangeNow();
            var toInsert = new List<StudentGoalSummary>();
            var courseGoalById = allCourseGoals.ToDictionary(g => g.Id);

            foreach (var ag in aggregates)
            {
                var hasThisWeek = ag.StudentGoalSummaries?.Any(s => s.StartDate >= weekStartUtc && s.EndDate <= weekEndUtc) == true;
                if (hasThisWeek)
                {
                    continue;
                }
                if (!courseGoalById.TryGetValue(ag.CourseGoalId, out var courseGoal))
                {
                    continue;
                }
                var cfg = courseGoal?.CourseGoalConfigs?.FirstOrDefault(c => c.Id == ag.CourseGoalConfigId);
                if (cfg == null)
                {
                    continue;
                }
                var stat = doneStats.TryGetValue((ag.StudentId, ag.CourseId), out var s) ? s : default;

                toInsert.Add(new StudentGoalSummary
                {
                    ProgressStatus = EnumCombinedProgressHelper.GetProgressStatusFromCounts(stat.week, cfg.LessonsPerWeek),
                    StartDate = weekStartUtc,
                    EndDate = weekEndUtc,
                    StudentGoalAggregateId = ag.Id,
                    CompletedLessons = stat.week,
                    LessonsPerWeek = cfg.LessonsPerWeek,
                    TotalTargetLessons = ag.TotalTargetLessons,
                    TotalCompletedLessons = ag.TotalCompletedLessons,
                    LastCompletedAt = stat.last
                });
            }

            if (toInsert.Count > 0)
            {
                await _studentLearningGoalSummaryRepository.AddList(toInsert);
                await _studentLearningGoalSummaryRepository.UnitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
            }
        }

        private async Task EnsureWeeklySummariesForAggregatesAsync(
            IList<StudentGoalAggregate> aggregates,
            IList<StudentModel> students,
            IList<CourseGoalModel> allCourseGoals,
            IDictionary<(Guid StudentId, Guid CourseId), (int total, int week, DateTime? last)> doneStats,
            CancellationToken ct)
        {
            if (aggregates.Count == 0)
            {
                return;
            }
            var (weekStartUtc, weekEndUtc) = Shared.Helpers.DateTimeHelper.GetCurrentWeekRangeNow();
            var toInsert = new List<StudentGoalSummary>();
            var updatedAggregates = new List<StudentGoalAggregate>();
            var courseGoalById = allCourseGoals.ToDictionary(g => g.Id);
            var courseStudents = aggregates.Select(a => new { a.CourseId, a.StudentId }).Distinct().ToList();

            var courseResults = await _courseResultRepository.Queryable
                .AsNoTracking()
                .WhereBulkContains(courseStudents, new[] { "CourseId", "StudentId" })
                .ToListAsync(ct);

            foreach (var ag in aggregates)
            {
                var courseResult = courseResults.FirstOrDefault(cr => cr.CourseId == ag.CourseId && cr.StudentId == ag.StudentId);
                if (courseResult != null && courseResult.Status == EnumResultStatus.Done)
                {
                    continue;
                }
                var student = students.FirstOrDefault(x => x.Id == ag.StudentId);
                if (student == null)
                {
                    continue;
                }
                var hasThisWeek = ag.StudentGoalSummaries?.Any(s => s.StartDate >= weekStartUtc && s.EndDate <= weekEndUtc) == true;
                if (hasThisWeek)
                {
                    continue;
                }
                var courseGoal = GetCourseGoal(allCourseGoals, ag.CourseLevel, ag.CourseType, student.SchoolClassId);
                var cfg = courseGoal?.CourseGoalConfigs.FirstOrDefault(x => x.CourseId == ag.CourseId);
                if (courseGoal == null || cfg == null)
                {
                    continue;
                }
                var stat = doneStats.TryGetValue((ag.StudentId, ag.CourseId), out var s) ? s : default;

                toInsert.Add(new StudentGoalSummary
                {
                    ProgressStatus = EnumCombinedProgressHelper.GetProgressStatusFromCounts(stat.week, cfg.LessonsPerWeek),
                    StartDate = weekStartUtc,
                    EndDate = weekEndUtc,
                    StudentGoalAggregateId = ag.Id,
                    LessonsPerWeek = cfg.LessonsPerWeek,
                    TotalTargetLessons = ag.TotalTargetLessons,
                    TotalCompletedLessons = ag.TotalCompletedLessons,
                    CompletedLessons = stat.week,
                    LastCompletedAt = stat.last
                });

                if (ag.CourseGoalConfigId != cfg.Id || ag.CourseGoalId != courseGoal.Id)
                {
                    ag.CourseGoalConfigId = cfg.Id;
                    ag.CourseGoalId = courseGoal.Id;
                    updatedAggregates.Add(ag);
                }
            }

            if (toInsert.Count > 0)
            {
                await _studentLearningGoalSummaryRepository.AddList(toInsert);
                await _studentLearningGoalSummaryRepository.UnitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
            }

            if (updatedAggregates.Any())
            {
                _studentLearningGoalAggregateRepository.UpdateList(updatedAggregates);
                await _studentLearningGoalAggregateRepository.UnitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
            }
        }

        private static CourseGoalModel? GetCourseGoal(IList<CourseGoalModel> courseGoals, EnumCourseLevel courseLevel, EnumCourseType courseType, Guid? schoolClassId = default)
        {
            var list = courseGoals.Where(x => x.CourseLevel == courseLevel && x.CourseType == courseType).ToList();
            if (schoolClassId.HasValue)
            {
                return list.FirstOrDefault(x => x.GoalCategory == EnumCourseGoalCategory.School && x.ClassId == schoolClassId.Value)
                    ?? list.FirstOrDefault(x => x.GoalCategory == EnumCourseGoalCategory.All);
            }
            else
            {
                return list.FirstOrDefault(x => x.GoalCategory == EnumCourseGoalCategory.DefaultExcludeSchoolAndClass)
                    ?? list.FirstOrDefault(x => x.GoalCategory == EnumCourseGoalCategory.All);
            }
        }

        // =========================
        // Rebuild progress (memory-light)
        // =========================

        private async Task RebuildStudentLearningProgressAsync(IList<CourseGoalModel> courseGoals, CancellationToken ct)
        {
            var today = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date;

            var aggregates = await _studentLearningGoalAggregateRepository.Queryable
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToListAsync(ct);
            if (aggregates.Count == 0)
            {
                return;
            }
            var latestWeeklyByAggId = await FetchLatestWeeklyByAggregateIdAsync(ct);
            var totalsByAggId = await LoadWeeklyTotalsAsync(latestWeeklyByAggId.Keys, today, ct);

            var studentIds = aggregates.Select(a => a.StudentId).Distinct().ToList();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            var studentMap = (studentResults.Content?.Result ?? new List<StudentModel>()).ToDictionary(s => s.Id);

            var pairs = aggregates.Select(a => (a.StudentId, a.CourseId)).Distinct().ToList();
            var doneStats = await LoadDoneLessonStatsAsync(pairs, ct);

            var weeklyToUpdate = new List<StudentGoalSummary>();
            foreach (var ag in aggregates)
            {
                if (!studentMap.ContainsKey(ag.StudentId))
                {
                    continue;
                }
                if (!latestWeeklyByAggId.TryGetValue(ag.Id, out var w) || w == null)
                {
                    continue;
                }
                var stat = doneStats.TryGetValue((ag.StudentId, ag.CourseId), out var s) ? s : default;
                var totals = totalsByAggId.TryGetValue(ag.Id, out var t) ? t : (0, 0);

                // cập nhật weekly mới nhất
                w.CompletedLessons = stat.week;
                w.TotalCompletedLessons = stat.total;
                w.LastCompletedAt = stat.last;
                w.ProgressStatus = EnumCombinedProgressHelper.GetProgressStatusFromCounts(w.CompletedLessons, w.LessonsPerWeek);
                weeklyToUpdate.Add(w);

                // cập nhật aggregate
                ag.TotalCompletedLessons = stat.total;

                if (w.StartDate <= today && w.EndDate >= today)
                {
                    ag.CombinedProgress = EnumCombinedProgressHelper.GetCurrentCombineProgress(totals.Item1, totals.Item2, w.ProgressStatus);
                    ag.CurrentCombinedProgress = ag.CombinedProgress;
                }
                else
                {
                    ag.CurrentCombinedProgress = EnumCombinedProgressHelper.GetCurrentCombineProgress(totals.Item1, totals.Item2, w.ProgressStatus);
                    ag.CombinedProgress = EnumCombinedProgressHelper.GetCombineProgress(totals.Item1, totals.Item2);
                }
            }

            // No direct DbContext from UnitOfWork → save in chunks instead of toggling AutoDetectChanges/clearing tracker
            const int chunkSizeUpdate = 2000;
            foreach (var chunk in weeklyToUpdate.Chunk(chunkSizeUpdate))
            {
                _studentLearningGoalSummaryRepository.UpdateList(chunk.ToList());
                await _studentLearningGoalSummaryRepository.UnitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
            }

            foreach (var chunk in aggregates.Chunk(chunkSizeUpdate))
            {
                _studentLearningGoalAggregateRepository.UpdateList(chunk.ToList());
                await _studentLearningGoalAggregateRepository.UnitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
            }
            await EnsureWeeklySummariesForAggregatesAsync(aggregates, studentMap.Values.ToList(), courseGoals, doneStats, ct).ConfigureAwait(false);
        }
    }
}

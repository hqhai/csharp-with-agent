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

    public class TestDataRebuildCommand : IRequest<MethodResult<bool>>
    {
    }

    public class TestDataRebuildCommandHandler : IRequestHandler<TestDataRebuildCommand, MethodResult<bool>>
    {
        private readonly IStudentGoalAggregateRepository _studentLearningGoalAggregateRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly IStudentGoalSummaryRepository _studentLearningGoalSummaryRepository;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;

        public TestDataRebuildCommandHandler(
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

        public async Task<MethodResult<bool>> Handle(TestDataRebuildCommand request, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            // 0) Load CourseGoals (no-tracking on caller side)
            var cgRes = await _systemService.GetListCourseGoalAsync().ConfigureAwait(false);
            var courseGoals = cgRes.Content?.Result ?? new List<CourseGoalModel>();

            // 1) Stream students by page (only CourseId not empty)
            await foreach (var studentPage in StreamStudentsByPageAsync(ct))
            {
                if (studentPage.Count == 0)
                {
                    continue;
                }
                await CreateStudentAggregateAsync(studentPage, courseGoals, ct).ConfigureAwait(false);
                // Không có DbContext trong UnitOfWork → không Clear() tracker; dùng AsNoTracking + save theo lô
            }

            // 2) Rebuild progress & ensure weekly (current + 3 prev weeks)
            await RebuildStudentLearningProgressAsync(courseGoals, ct).ConfigureAwait(false);

            methodResult.Result = true;
            return methodResult;
        }

        // =============================
        // Streaming students (memory-safe)
        // =============================
        private async IAsyncEnumerable<IList<StudentDetailModel>> StreamStudentsByPageAsync(
            [EnumeratorCancellation] CancellationToken ct)
        {
            const int pageSize = 5000; // tune per RAM/DB
            var filters = new List<GenericFilterModel>
            {
                new() { Property = nameof(StudentModel.SchoolClassId), Operator = EnumFilterOperator.NotEmpty }
            };

            var page = 1;
            while (true)
            {
                ct.ThrowIfCancellationRequested();

                var q = new BaseQueryModel { Page = page, PageSize = pageSize, Filters = filters };
                var resp = await _userService.SearchAsync(q).ConfigureAwait(false);
                var result = resp?.Content?.Result;
                var items = result?.Items;

                if (items == null || items.Count == 0)
                {
                    yield break;
                }
                yield return items;

                page++;
                var totalPages = result?.PagingInfo?.TotalItems / pageSize;
                if (totalPages.HasValue && page > totalPages.Value)
                {
                    yield break;
                }
            }
        }

        // =============================
        // Loaders (AsNoTracking) & helpers
        // =============================

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

        private async Task<Dictionary<Guid, int>> LoadCourseLessonCountsAsync(IEnumerable<Guid> courseIds, CancellationToken ct)
        {
            var ids = courseIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return new();
            }
            var rows = await _courseUnitMockTestRepository.Queryable
                .AsNoTracking()
                .WhereBulkContains(ids, x => x.CourseId)
                .Select(x => new { x.CourseId, Count = x.Unit!.UnitLessons.Count })
                .ToListAsync(ct);

            return rows.GroupBy(x => x.CourseId).ToDictionary(g => g.Key, g => g.Sum(x => x.Count));
        }

        private async Task<Dictionary<(Guid StudentId, Guid CourseId), IEnumerable<LessonResult>>> LoadDoneLessonResultsMapAsync(
            IList<CourseResultModel> aggregates,
            CancellationToken ct)
        {
            var studentCourseKeys = aggregates
                .Select(a => new { a.StudentId, a.CourseId })
                .Distinct()
                .ToList();

            var rows = await _lessonResultRepository.Queryable
                .AsNoTracking()
                .Where(x => x.Status == EnumResultStatus.Done)
                .WhereBulkContains(studentCourseKeys, new[] { "StudentId", "CourseId" })
                .Select(x => new LessonResult
                {
                    StudentId = x.StudentId,
                    CourseId = x.CourseId,
                    CompletionDate = x.CompletionDate,
                    UpdatedDate = x.UpdatedDate,
                    CreatedDate = x.CreatedDate,
                })
                .ToListAsync(ct);

            return rows
                .GroupBy(x => (x.StudentId, x.CourseId))
                .ToDictionary(g => g.Key, g => g.AsEnumerable());
        }

        // =============================
        // Create aggregates (page-batch)
        // =============================
        private async Task CreateStudentAggregateAsync(
            IList<StudentDetailModel> students,
            IList<CourseGoalModel> courseGoals,
            CancellationToken ct)
        {
            var studentIds = students.Select(x => x.Id).ToList();
            var unlinkedCourseResults = await GetUnlinkedCourseResultsAsync(studentIds, ct).ConfigureAwait(false);
            var unlinkedStudentIds = unlinkedCourseResults.Select(x => x.StudentId).Distinct().ToHashSet();

            var studentIdsWithCourseResult = await GetStudentIdsWithCourseResultAsync(studentIds, ct).ConfigureAwait(false);

            var studentsToProcess = students
                .Where(s => s.CourseId.HasValue && (!studentIdsWithCourseResult.Contains(s.Id) || unlinkedStudentIds.Contains(s.Id)))
                .ToList();

            if (studentsToProcess.Count == 0)
            {
                return;
            }
            var courseIds = studentsToProcess.Where(x => x.CourseId.HasValue).Select(s => s.CourseId!.Value).Distinct().ToList();
            var totalLessonsPerCourse = await LoadCourseLessonCountsAsync(courseIds, ct).ConfigureAwait(false);

            var courseResults = studentsToProcess
                .Select(s => new CourseResultModel { StudentId = s.Id, CourseId = s.CourseId ?? default })
                .ToList();

            var doneLessonResultsMap = await LoadDoneLessonResultsMapAsync(courseResults, ct).ConfigureAwait(false);

            var aggregatesToInsert = BuildAggregates(courseResults, courseGoals, students, totalLessonsPerCourse, doneLessonResultsMap);
            if (aggregatesToInsert.Count == 0)
            {
                return;
            }
            // Insert theo lô (không cần Clear tracker)
            const int batch = 2000;
            for (int i = 0; i < aggregatesToInsert.Count; i += batch)
            {
                var slice = aggregatesToInsert.Skip(i).Take(batch).ToList();
                await _studentLearningGoalAggregateRepository.AddList(slice);
                await _studentLearningGoalAggregateRepository.UnitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
            }

            // Tạo weekly cho tuần hiện tại + 3 tuần trước
            await EnsureWeeklySummariesForAggregatesAsync(aggregatesToInsert, courseGoals, doneLessonResultsMap, ct).ConfigureAwait(false);
        }

        private static List<StudentGoalAggregate> BuildAggregates(
            IEnumerable<CourseResultModel> courseResults,
            IList<CourseGoalModel> courseGoals,
            IList<StudentDetailModel> students,
            IDictionary<Guid, int> totalLessonsPerCourse,
            IDictionary<(Guid StudentId, Guid CourseId), IEnumerable<LessonResult>> doneLessonResultsMap)
        {
            var aggregates = new List<StudentGoalAggregate>();
            var studentMap = students.ToDictionary(s => s.Id);

            foreach (var cr in courseResults)
            {
                if (!studentMap.TryGetValue(cr.StudentId, out var student))
                {
                    continue;
                }
                var level = cr.CourseLevel ?? student.CourseLevel ?? default;
                var type = level.GetEnumCourseType();

                var courseGoal = GetCourseGoal(courseGoals, level, type, student.SchoolClassId);
                var cfg = courseGoal?.CourseGoalConfigs.FirstOrDefault(x => x.CourseId == cr.CourseId);
                if (courseGoal == null || cfg == null)
                {
                    continue;
                }
                var totalTarget = totalLessonsPerCourse.TryGetValue(cr.CourseId, out var total) ? total : 0;
                var results = doneLessonResultsMap.TryGetValue((cr.StudentId, cr.CourseId), out var r)
                    ? r
                    : Enumerable.Empty<LessonResult>();

                var ag = new StudentGoalAggregate
                {
                    CombinedProgress = EnumCombinedProgress.TotalBehindWeekBehind,
                    CurrentCombinedProgress = EnumCombinedProgress.TotalBehindWeekBehind,
                    StudentId = cr.StudentId,
                    CourseId = cr.CourseId,
                    CourseGoalId = courseGoal.Id,
                    TotalCompletedLessons = results.Count(),
                    CourseGoalConfigId = cfg.Id,
                    TotalTargetLessons = totalTarget,
                    CourseLevel = level,
                    IsActive = true,
                    CourseType = type,
                };

                if (student.SchoolClassId.HasValue)
                {
                    ag.SchoolId = student.SchoolId;
                    ag.SchoolName = student.School;
                    ag.ClassId = student.SchoolClassId;
                    ag.ClassName = student.SchoolClass;
                }

                aggregates.Add(ag);
            }

            return aggregates;
        }

        // =============================
        // Ensure weekly summaries (current + 3 previous weeks)
        // =============================
        private async Task EnsureWeeklySummariesForAggregatesAsync(
            IList<StudentGoalAggregate> aggregates,
            IList<CourseGoalModel> allCourseGoals,
            IDictionary<(Guid StudentId, Guid CourseId), IEnumerable<LessonResult>> doneLessonResultsMap,
            CancellationToken ct)
        {
            if (aggregates == null || aggregates.Count == 0)
            {
                return;
            }
            var toInsert = new List<StudentGoalSummary>();
            var courseGoalById = allCourseGoals.ToDictionary(g => g.Id);

            foreach (var ag in aggregates)
            {
                for (var i = 0; i <= 3; i++)
                {
                    var (weekStartUtc, weekEndUtc) = GetWeekRangeUtc(i);

                    var hasThisWeek = ag.StudentGoalSummaries?.Any(s => s.StartDate.Date == weekStartUtc.Date && s.EndDate.Date == weekEndUtc.Date) == true;
                    if (hasThisWeek)
                    {
                        continue;
                    }
                    if (!courseGoalById.TryGetValue(ag.CourseGoalId, out var courseGoal))
                    {
                        continue;
                    }
                    var cfg = courseGoal.CourseGoalConfigs.FirstOrDefault(c => c.Id == ag.CourseGoalConfigId);
                    if (cfg == null)
                    {
                        continue;
                    }
                    var results = doneLessonResultsMap.TryGetValue((ag.StudentId, ag.CourseId), out var r)
                        ? r
                        : Enumerable.Empty<LessonResult>();

                    var completedThisWeek = results.Count(x =>
                    {
                        var d = (x.CompletionDate ?? x.UpdatedDate ?? x.CreatedDate).Date;
                        return d >= weekStartUtc && d <= weekEndUtc;
                    });

                    toInsert.Add(new StudentGoalSummary
                    {
                        ProgressStatus = EnumCombinedProgressHelper.GetProgressStatusFromCounts(completedThisWeek, cfg.LessonsPerWeek),
                        StartDate = weekStartUtc,
                        EndDate = weekEndUtc,
                        StudentGoalAggregateId = ag.Id,
                        CompletedLessons = completedThisWeek,
                        LessonsPerWeek = cfg.LessonsPerWeek,
                        TotalTargetLessons = ag.TotalTargetLessons,
                        TotalCompletedLessons = ag.TotalCompletedLessons,
                    });
                }
            }

            if (toInsert.Count > 0)
            {
                const int chunk = 2000;
                for (int i = 0; i < toInsert.Count; i += chunk)
                {
                    var slice = toInsert.Skip(i).Take(chunk).ToList();
                    await _studentLearningGoalSummaryRepository.AddList(slice);
                    await _studentLearningGoalSummaryRepository.UnitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
                }
            }
        }

        // =============================
        // Rebuild progress using latest weekly + totals
        // =============================
        private async Task RebuildStudentLearningProgressAsync(IList<CourseGoalModel> courseGoals, CancellationToken ct)
        {
            var today = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date;

            // Lấy cặp (Aggregate, Weekly) đang active
            var aggAndWeekly = await FetchActiveAggWeeklyPairsAsync(ct).ConfigureAwait(false);
            var aggregates = aggAndWeekly.Select(p => p.Aggregate).Distinct().ToList();
            var weeklySummaries = aggAndWeekly.Select(p => p.Weekly).ToList();

            // Latest weekly per aggregate
            var latestWeeklyByAggId = PickLatestWeeklyByAggregateId(weeklySummaries);

            // Total weekly done/plan up to today
            var totalsByAggId = await LoadWeeklyTotalsAsync(latestWeeklyByAggId.Keys, today, ct).ConfigureAwait(false);

            // Build courseResults from aggregates to fetch lesson results
            var courseResults = aggregates.Select(cr => new CourseResultModel { StudentId = cr.StudentId, CourseId = cr.CourseId }).ToList();
            var doneLessonResultsMap = await LoadDoneLessonResultsMapAsync(courseResults, ct).ConfigureAwait(false);

            var weeklyToUpdate = new List<StudentGoalSummary>();
            foreach (var ag in aggregates)
            {
                if (!latestWeeklyByAggId.TryGetValue(ag.Id, out var weekly) || weekly is null)
                {
                    continue;
                }
                var results = doneLessonResultsMap.TryGetValue((ag.StudentId, ag.CourseId), out var r) ? r : Enumerable.Empty<LessonResult>();

                // Update weekly + totals
                ApplyWeeklyAndTotalProgress(weekly, ag, results);

                // Update aggregate combined/current status from totals
                var totals = totalsByAggId.TryGetValue(ag.Id, out var t) ? t : (0, 0);
                if (weekly.StartDate <= today && weekly.EndDate >= today)
                {
                    ag.CombinedProgress = EnumCombinedProgressHelper.GetCurrentCombineProgress(totals.Item1, totals.Item2, weekly.ProgressStatus);
                    ag.CurrentCombinedProgress = ag.CombinedProgress;
                }
                else
                {
                    ag.CurrentCombinedProgress = EnumCombinedProgressHelper.GetCurrentCombineProgress(totals.Item1, totals.Item2, weekly.ProgressStatus);
                    ag.CombinedProgress = EnumCombinedProgressHelper.GetCombineProgress(totals.Item1, totals.Item2);
                }

                weeklyToUpdate.Add(weekly);

                // Streak
                var weeklies = weeklySummaries.Where(x => x.StudentGoalAggregateId == ag.Id && x.EndDate <= today).ToList();
                UpdateBehindStreak(ag, today, weeklies);
            }

            // Save updates in chunks
            const int chunkSize = 2000;
            foreach (var chunk in weeklyToUpdate.Chunk(chunkSize))
            {
                _studentLearningGoalSummaryRepository.UpdateList(chunk.ToList());
                await _studentLearningGoalSummaryRepository.UnitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
            }

            foreach (var chunk in aggregates.Chunk(chunkSize))
            {
                _studentLearningGoalAggregateRepository.UpdateList(chunk.ToList());
                await _studentLearningGoalAggregateRepository.UnitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
            }

            var studentIds = aggregates.Select(a => a.StudentId).Distinct().ToList();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds).ConfigureAwait(false);
            var students = studentResults.Content?.Result ?? new List<StudentModel>();

            await EnsureWeeklySummariesForAggregatesAsync(aggregates, students, courseGoals, doneLessonResultsMap, ct).ConfigureAwait(false);
        }

        private async Task EnsureWeeklySummariesForAggregatesAsync(IList<StudentGoalAggregate> aggregates, IList<StudentModel> students, IList<CourseGoalModel> allCourseGoals, IDictionary<(Guid StudentId, Guid CourseId), IEnumerable<LessonResult>> doneLessonResultsMap, CancellationToken ct)
        {
            if (aggregates == null || aggregates.Count == 0)
            {
                return;
            }
            var toInsert = new List<StudentGoalSummary>();
            var updatedAggregates = new List<StudentGoalAggregate>();
            var courseStudents = aggregates.Select(a => new { a.CourseId, a.StudentId }).Distinct().ToList();
            var courseResults = await _courseResultRepository.Queryable.WhereBulkContains(courseStudents, new[] { "CourseId", "StudentId" }).AsNoTracking().ToListAsync(ct);
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
                for (var i = 0; i <= 3; i++)
                {
                    var (weekStartUtc, weekEndUtc) = GetWeekRangeUtc(i);
                    var hasThisWeek = ag.StudentGoalSummaries?.Any(s => s.StartDate.Date == weekStartUtc.Date && s.EndDate.Date == weekEndUtc.Date) == true;
                    if (hasThisWeek)
                    {
                        continue;
                    }
                    var courseGoal = GetCourseGoal(allCourseGoals, ag.CourseLevel, ag.CourseType, student.SchoolClassId);
                    var courseGoalConfig = courseGoal?.CourseGoalConfigs.FirstOrDefault(x => x.CourseId == ag.CourseId);
                    if (courseGoal == null || courseGoalConfig == null)
                    {
                        continue;
                    }
                    var results = doneLessonResultsMap.TryGetValue((ag.StudentId, ag.CourseId), out var r) ? r : Enumerable.Empty<LessonResult>();
                    var completedThisWeek = results.Count(x => { var d = (x.CompletionDate ?? x.UpdatedDate ?? x.CreatedDate).Date; return d >= weekStartUtc && d <= weekEndUtc; });
                    toInsert.Add(new StudentGoalSummary { ProgressStatus = EnumCombinedProgressHelper.GetProgressStatusFromCounts(completedThisWeek, courseGoalConfig.LessonsPerWeek), StartDate = weekStartUtc, EndDate = weekEndUtc, StudentGoalAggregateId = ag.Id, LessonsPerWeek = courseGoalConfig.LessonsPerWeek, TotalTargetLessons = ag.TotalTargetLessons, TotalCompletedLessons = ag.TotalCompletedLessons, });
                    if (ag.CourseGoalConfigId != courseGoalConfig.Id || ag.CourseGoalId != courseGoal.Id)
                    {
                        ag.CourseGoalConfigId = courseGoalConfig.Id;
                        ag.CourseGoalId = courseGoal.Id;
                        updatedAggregates.Add(ag);
                    }
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

        private static void ApplyWeeklyAndTotalProgress(
            StudentGoalSummary weekly,
            StudentGoalAggregate agg,
            IEnumerable<LessonResult> results)
        {
            var last = results.OrderByDescending(x => x.CompletionDate ?? x.UpdatedDate).FirstOrDefault();
            weekly.LastCompletedAt = last?.CompletionDate ?? last?.UpdatedDate;

            var (weekStartUtc, weekEndUtc) = GetWeekRangeUtc(0);

            var completedThisWeek = results.Count(x =>
            {
                var d = (x.CompletionDate ?? x.UpdatedDate);
                return d >= weekStartUtc && d <= weekEndUtc;
            });

            weekly.CompletedLessons = completedThisWeek;
            weekly.ProgressStatus = EnumCombinedProgressHelper.GetProgressStatusFromCounts(completedThisWeek, weekly.LessonsPerWeek);

            var totalCompleted = results.Count();
            weekly.TotalCompletedLessons = totalCompleted;
            agg.TotalCompletedLessons = totalCompleted;
        }

        private static void UpdateBehindStreak(StudentGoalAggregate agg, DateTime nowVn, IReadOnlyList<StudentGoalSummary> weeklySummaries)
        {
            if (weeklySummaries == null || weeklySummaries.Count == 0)
            {
                agg.ConsecutiveBehindWeeks = 0;
                return;
            }

            var ordered = weeklySummaries
                .Where(w => w != null && w.EndDate < nowVn)
                .OrderBy(w => w.EndDate != default ? w.EndDate
                         : w.StartDate != default ? w.StartDate
                         : w.CreatedDate)
                .ToList();

            var streak = 0;
            for (int i = ordered.Count - 1; i >= 0; i--)
            {
                var status = ordered[i].ProgressStatus;
                if (status == EnumProgressStatus.Behind)
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

        private sealed record AggWeeklyPair(StudentGoalAggregate Aggregate, StudentGoalSummary Weekly);

        private async Task<List<AggWeeklyPair>> FetchActiveAggWeeklyPairsAsync(CancellationToken ct)
        {
            return await (
                from ag in _studentLearningGoalAggregateRepository.Queryable.Where(x => x.IsActive)
                join sm in _studentLearningGoalSummaryRepository.Queryable
                     on ag.Id equals sm.StudentGoalAggregateId
                select new AggWeeklyPair(ag, sm)
            ).ToListAsync(ct);
        }

        private static Dictionary<Guid, StudentGoalSummary> PickLatestWeeklyByAggregateId(IEnumerable<StudentGoalSummary> weeklies)
        {
            return weeklies
                .GroupBy(s => s.StudentGoalAggregateId)
                .Select(g => g.OrderByDescending(x => x.StartDate).First())
                .ToDictionary(x => x.StudentGoalAggregateId, x => x);
        }

        private async Task<Dictionary<Guid, (int totalDone, int totalPlan)>> LoadWeeklyTotalsAsync(IEnumerable<Guid> aggregateIds, DateTime today, CancellationToken ct)
        {
            var ids = aggregateIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return new();
            }
            var rows = await _studentLearningGoalSummaryRepository.Queryable
                .AsNoTracking()
                .Where(x => ids.Contains(x.StudentGoalAggregateId) && x.EndDate <= today)
                .GroupBy(x => x.StudentGoalAggregateId)
                .Select(g => new { AggregateId = g.Key, TotalDone = g.Sum(s => s.CompletedLessons), TotalPlan = g.Sum(s => s.LessonsPerWeek) })
                .ToListAsync(ct)
                .ConfigureAwait(false);

            return rows.ToDictionary(k => k.AggregateId, v => (v.TotalDone, v.TotalPlan));
        }

        private static (DateTime weekStartUtc, DateTime weekEndUtc) GetWeekRangeUtc(int weeksAgo = 0)
        {
            var nowUtc = DateTime.UtcNow;
            var nowVn = nowUtc.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date;
            if (weeksAgo > 0)
            {
                nowVn = nowVn.AddDays(-7 * weeksAgo);
            }
            var startVn = GetWeekStartMonday(nowVn);
            var endVn = startVn.AddDays(6);
            return (startVn, endVn);
        }

        private static DateTime GetWeekStartMonday(DateTime date)
        {
            var day = (int)date.DayOfWeek; // Sunday = 0
            var offset = day == 0 ? -6 : 1 - day;
            return date.AddDays(offset);
        }

        private static CourseGoalModel? GetCourseGoal(
            IList<CourseGoalModel> courseGoals,
            EnumCourseLevel courseLevel,
            EnumCourseType courseType,
            Guid? schoolClassId = default)
        {
            var list = courseGoals.Where(x => x.CourseLevel == courseLevel && x.CourseType == courseType).ToList();
            if (schoolClassId.HasValue)
            {
                return list.FirstOrDefault(x => x.GoalCategory == EnumCourseGoalCategory.School && x.ClassId == schoolClassId.Value)
                       ?? list.FirstOrDefault(x => x.GoalCategory == EnumCourseGoalCategory.All);
            }
            return list.FirstOrDefault(x => x.GoalCategory == EnumCourseGoalCategory.DefaultExcludeSchoolAndClass)
                   ?? list.FirstOrDefault(x => x.GoalCategory == EnumCourseGoalCategory.All);
        }
    }
}

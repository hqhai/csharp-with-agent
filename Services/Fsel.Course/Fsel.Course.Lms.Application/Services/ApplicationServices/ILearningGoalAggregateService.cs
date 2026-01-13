namespace Fsel.Course.Lms.Application.Services.ApplicationServices
{
    using System;
    using Fsel.Common.Enums;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

    public interface ILearningGoalAggregateService
    {
        Task<HashSet<(Guid StudentId, Guid CourseId)>> GetAggregateKeysAsync(
        IReadOnlyCollection<(Guid StudentId, Guid CourseId)> keys,
        CancellationToken ct);

        Task<Dictionary<(Guid StudentId, Guid CourseId), LessonCompletionStats>> GetLessonCompletionStatsAsync(
        IReadOnlyCollection<(Guid, Guid)> keys,
        DateRangeExclusive weekRangeExclusive,
        CancellationToken ct);

        Task<IReadOnlyList<CourseResult>> GetCourseResultsWithoutAggregateAsync(
        IReadOnlyCollection<Guid> studentIds,
        CancellationToken ct = default);

        Task<IReadOnlyList<Guid>> GetStudentIdsWithCourseResultsAsync(
        IReadOnlyCollection<Guid> studentIds,
        CancellationToken ct = default);

        Task<Dictionary<(Guid StudentId, Guid CourseId), int>> LoadCourseLessonCountsAsync(
        IReadOnlyCollection<(Guid StudentId, Guid CourseId)> keys,
        CancellationToken ct);

        Task<Dictionary<Guid, (int totalDone, int totalPlan)>> LoadWeeklyTotalsAsync(
        IEnumerable<Guid> aggregateIds,
        DateRange weekRange,
        CancellationToken ct);

        Task<IDictionary<Guid, IReadOnlyList<EnumProgressStatus>>> GetOrderedProgressStatusesBeforeAsync(
        DateRange weekRange,
        CancellationToken cancellationToken = default);
    }

    public readonly record struct LessonCompletionStats(
        int TotalCompleted,
        int CompletedThisWeek,
        DateTime? LastCompletedAt);
    public record DateRangeExclusive(DateTime WeekStart, DateTime WeekEndExclusiveUtc);
    public record DateRange(DateTime WeekStart, DateTime WeekEnd);

    public class LearningGoalAggregateService : ILearningGoalAggregateService
    {
        private readonly ILessonResultRepository _lessonResults;
        private readonly IStudentGoalAggregateRepository _aggregates;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseModuleRepository _courseModuleRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IStudentGoalSummaryRepository _studentGoalSummary;

        public LearningGoalAggregateService(
            ILessonResultRepository lessonResults,
            IStudentGoalAggregateRepository aggregates,
            ICourseResultRepository courseResultRepository,
            ICourseModuleRepository courseModuleRepository,
            IUnitResultRepository unitResultRepository,
            IUnitRepository unitRepository,
            IStudentGoalSummaryRepository studentGoalSummary)
        {
            _lessonResults = lessonResults;
            _aggregates = aggregates;
            _courseResultRepository = courseResultRepository;
            _courseModuleRepository = courseModuleRepository;
            _unitResultRepository = unitResultRepository;
            _unitRepository = unitRepository;
            _studentGoalSummary = studentGoalSummary;
        }

        public async Task<Dictionary<(Guid StudentId, Guid CourseId), int>> LoadCourseLessonCountsAsync(
        IReadOnlyCollection<(Guid StudentId, Guid CourseId)> keys,
        CancellationToken ct)
        {
            if (keys == null || keys.Count == 0)
            {
                return new Dictionary<(Guid StudentId, Guid CourseId), int>();
            }
            // Distinct courseIds để query gọn
            var courseIds = keys.Select(x => x.CourseId).Distinct().ToList();

            // 1) Lấy danh sách Unit thuộc Course (config type = Unit) + lesson count theo LastVersion
            var queryModule = _courseModuleRepository.ReadQueryable
                                                     .Where(x => x.CourseConfigType == EnumCourseConfigType.Unit)
                                                     .WhereBulkContains(courseIds, x => x.CourseId);

            var courseUnits = await (from cm in queryModule
                                     join u in _unitRepository.ReadQueryable on cm.OriginalId equals u.OriginalId
                                     where u.VersionStatus == EnumVersionStatus.LastVersion
                                     select new
                                     {
                                         cm.CourseId,
                                         UnitOriginalId = u.OriginalId,
                                         TotalLesson = u.LessonCount
                                     }).AsNoTracking().ToListAsync(ct);

            // Course -> list units (OriginalId + LessonCount)
            var unitsByCourse = courseUnits
                .GroupBy(x => x.CourseId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => (x.UnitOriginalId, x.TotalLesson)).ToList()
                );

            // 2) Lấy CourseResult theo keys
            var courseResultsQuery = _courseResultRepository.ReadQueryable
                .Where(x => x.WorkingStatus == EnumWorkingStatus.Active)
                .AsNoTracking()
                .WhereBulkContains(keys, new[] { "StudentId", "CourseId" })
                .Select(cr => new { cr.Id, cr.StudentId, cr.CourseId });

            // 3) Lấy UnitResult và map theo (StudentId, CourseId, UnitOriginalId) -> LessonCount
            var unitResults = await (from cr in courseResultsQuery
                                     join ur in _unitResultRepository.ReadQueryable.AsNoTracking()
                                         on new { cr.CourseId, cr.StudentId, CourseResultId = (Guid?)cr.Id }
                                         equals new { ur.CourseId, ur.StudentId, ur.CourseResultId }
                                     where ur.Unit != null
                                     select new
                                     {
                                         cr.StudentId,
                                         cr.CourseId,
                                         UnitOriginalId = ur.Unit.OriginalId,
                                         TotalLesson = ur.Unit.LessonCount
                                     }).ToListAsync(ct);

            var unitLessonByStudentCourseUnit = unitResults
                .GroupBy(x => (x.StudentId, x.CourseId, x.UnitOriginalId))
                .ToDictionary(g => g.Key, g => g.First().TotalLesson);

            // 4) Build kết quả
            var result = new Dictionary<(Guid StudentId, Guid CourseId), int>(keys.Count);

            foreach (var key in keys)
            {
                if (!unitsByCourse.TryGetValue(key.CourseId, out var units))
                {
                    result[key] = 0;
                    continue;
                }

                var sum = 0;
                foreach (var (unitOriginalId, defaultLesson) in units)
                {
                    if (unitLessonByStudentCourseUnit.TryGetValue((key.StudentId, key.CourseId, unitOriginalId), out var overrideLesson))
                    {
                        sum += overrideLesson;
                    }
                    else
                    {
                        sum += defaultLesson;
                    }
                }

                result[key] = sum;
            }

            return result;
        }

        public async Task<IReadOnlyList<CourseResult>> GetCourseResultsWithoutAggregateAsync(
        IReadOnlyCollection<Guid> studentIds,
        CancellationToken ct = default)
        {
            if (studentIds == null || studentIds.Count == 0)
            {
                return Array.Empty<CourseResult>();
            }
            var query = _courseResultRepository.ReadQueryable
                .AsNoTracking()
                .WhereBulkContains(studentIds, x => x.StudentId)
                .Where(cr => cr.WorkingStatus != EnumWorkingStatus.NotWorking)
                .Where(cr => cr.Status != EnumResultStatus.Done)
                .Where(cr => !_aggregates.ReadQueryable.Any(ag =>
                    ag.IsActive
                    && ag.CourseId == cr.CourseId
                    && ag.StudentId == cr.StudentId
                    && ag.CourseResultId == cr.Id))
                .Include(cr => cr.Course); // giữ nếu caller thật sự cần Course

            return await query.ToListAsync(ct);
        }

        public async Task<HashSet<(Guid StudentId, Guid CourseId)>> GetAggregateKeysAsync(
        IReadOnlyCollection<(Guid StudentId, Guid CourseId)> keys,
        CancellationToken ct)
        {
            if (keys == null || keys.Count == 0)
            {
                return new();
            }
            var keyModels = keys.Select(x => new { x.StudentId, x.CourseId }).ToList();

            var rows = await _aggregates.ReadQueryable
                .AsNoTracking()
                .WhereBulkContains(keyModels, new[] { "StudentId", "CourseId" })
                .Select(a => new { a.StudentId, a.CourseId })
                .ToListAsync(ct);

            return rows.Select(x => (x.StudentId, x.CourseId)).ToHashSet();
        }

        public async Task<Dictionary<(Guid StudentId, Guid CourseId), LessonCompletionStats>> GetLessonCompletionStatsAsync(
        IReadOnlyCollection<(Guid, Guid)> keys,
        DateRangeExclusive weekRangeExclusive,
        CancellationToken ct)
        {
            if (keys == null || keys.Count == 0)
            {
                return new();
            }
            // Nếu team bạn thống nhất UTC: nên dùng 1 helper trả UTC range rõ ràng
            var (weekStartUtc, weekEndExclusiveUtc) = weekRangeExclusive;

            var keyModels = keys.Select(x => new { x.Item1, x.Item2 }).ToList();

            var rows = await _lessonResults.ReadQueryable
                .AsNoTracking()
                .Where(x => x.Status == EnumResultStatus.Done)
                .Where(x => (x.CompletionDate ?? x.UpdatedDate ?? x.CreatedDate) < weekEndExclusiveUtc)
                .WhereBulkContains(keyModels, new[] { "StudentId", "CourseId" })
                .Select(x => new
                {
                    x.StudentId,
                    x.CourseId,
                    CompletedAt = x.CompletionDate ?? x.UpdatedDate ?? x.CreatedDate
                })
                .GroupBy(x => new { x.StudentId, x.CourseId })
                .Select(g => new
                {
                    g.Key.StudentId,
                    g.Key.CourseId,
                    TotalCompleted = g.Count(),
                    CompletedThisWeek = g.Count(r => r.CompletedAt >= weekStartUtc && r.CompletedAt < weekEndExclusiveUtc),
                    LastCompletedAt = g.Max(r => r.CompletedAt)
                })
                .ToListAsync(ct);

            var result = new Dictionary<(Guid StudentId, Guid CourseId), LessonCompletionStats>(keys.Count);
            foreach (var r in rows)
            {
                result[(r.StudentId, r.CourseId)] =
                    new LessonCompletionStats(r.TotalCompleted, r.CompletedThisWeek, r.LastCompletedAt);
            }

            return result;
        }

        public async Task<IReadOnlyList<Guid>> GetStudentIdsWithCourseResultsAsync(
        IReadOnlyCollection<Guid> studentIds,
        CancellationToken ct = default)
        {
            if (studentIds == null || studentIds.Count == 0)
            {
                return Array.Empty<Guid>();
            }
            return await _courseResultRepository.ReadQueryable.Where(x => x.WorkingStatus != EnumWorkingStatus.NotWorking)
                .AsNoTracking()
                .WhereBulkContains(studentIds, x => x.StudentId)
                .Select(x => x.StudentId)
                .Distinct()
                .ToListAsync(ct);
        }

        public async Task<Dictionary<Guid, (int totalDone, int totalPlan)>> LoadWeeklyTotalsAsync(
        IEnumerable<Guid> aggregateIds,
        DateRange weekRange,
        CancellationToken ct)
        {
            var ids = aggregateIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return new();
            }
            var rows = await _studentGoalSummary.ReadQueryable
                .AsNoTracking()
                .Where(x => ids.Contains(x.StudentGoalAggregateId) && x.EndDate < weekRange.WeekEnd)
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

        public async Task<IDictionary<Guid, IReadOnlyList<EnumProgressStatus>>> GetOrderedProgressStatusesBeforeAsync(
        DateRange weekRange,
        CancellationToken cancellationToken = default)
        {
            // 1. Query: filter + sort theo AggregateId + mốc thời gian
            var items = await _studentGoalSummary.Queryable
                .AsNoTracking()
                .Where(x => x.EndDate < weekRange.WeekEnd)
                .Select(x => new
                {
                    x.StudentGoalAggregateId,
                    x.StartDate,
                    x.ProgressStatus
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            // 2. Group lại theo aggregate, giữ nguyên thứ tự đã sort
            var result = items
                .GroupBy(x => x.StudentGoalAggregateId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<EnumProgressStatus>)g.OrderBy(x => x.StartDate)
                            .Select(x => x.ProgressStatus)
                            .ToList());

            return result;
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.OtherFeatureCmd
{
    using System.Collections.Generic;
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
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

        public RebuildLearningGoalAggregateCommandHandler(IStudentGoalAggregateRepository studentLearningGoalAggregateRepository,
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
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var courseGoalRes = await _systemService.GetListCourseGoalAsync();
            var courseGoals = courseGoalRes.Content?.Result ?? new List<CourseGoalModel>();

            await RebuildStudentLearningProgressAsync(courseGoals, cancellationToken);
            await CreateStudentLearningGoalsAsync(courseGoals, cancellationToken);
            methodResult.Result = true;
            return methodResult;
        }

        public async Task CreateStudentLearningGoalsAsync(IList<CourseGoalModel> courseGoals, CancellationToken ct = default)
        {
            // 1) Lấy CourseResult chưa liên kết Aggregate
            var unlinkedCourseResults = await GetUnlinkedCourseResultsAsync(ct);
            if (!unlinkedCourseResults.Any())
            {
                return;
            }
            // 2) Lấy CourseGoal + đếm tổng lesson của các course liên quan

            var courseIds = unlinkedCourseResults.Select(c => c.CourseId).Distinct().ToList();
            var studentIds = unlinkedCourseResults.Select(c => c.StudentId).Distinct().ToList();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            var students = studentResults.Content?.Result ?? new List<StudentModel>();

            var totalLessonsPerCourse = await LoadCourseLessonCountsAsync(courseIds, ct);

            // 3) Build aggregates từ dữ liệu trên
            var aggregatesToInsert = BuildAggregates(unlinkedCourseResults, courseGoals, students, totalLessonsPerCourse);

            if (aggregatesToInsert.Count == 0)
            {
                return;
            }
            // 4) Persist aggregates
            await _studentLearningGoalAggregateRepository.AddList(aggregatesToInsert);
            await _studentLearningGoalAggregateRepository.UnitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

            // 5) Đọc lại aggregates vừa tạo (đã include summaries)
            var createdAggregateIds = aggregatesToInsert.Select(a => a.Id).ToList();

            var aggregates = await _studentLearningGoalAggregateRepository.Queryable
                .AsNoTracking() // chỉ đọc để tạo summary
                .Include(x => x.StudentGoalSummaries)
                .WhereBulkContains(createdAggregateIds, x => x.Id)
                .Where(x => x.IsActive)
                .ToListAsync(ct);

            // 6) Tạo weekly summaries cho tuần hiện tại nếu thiếu
            await EnsureWeeklySummariesForAggregatesAsync(aggregates, courseGoals, ct);
        }

        private static List<StudentGoalAggregate> BuildAggregates(
            IEnumerable<CourseResult> courseResults,
            IList<CourseGoalModel> courseGoals,
            IList<StudentModel> students,
            IDictionary<Guid, int> totalLessonsPerCourse)
        {
            var aggregates = new List<StudentGoalAggregate>();

            foreach (var cr in courseResults)
            {
                var student = students.FirstOrDefault(x => x.Id == cr.StudentId);
                if (student == null)
                {
                    continue;
                }
                var level = cr.Course!.CourseLevel;
                var type = cr.Course!.CourseType;

                // Chọn CourseGoal đúng ưu tiên (đã có helper GetCourseGoal* của bạn)
                var courseGoal = GetCourseGoal(courseGoals, level, type, student.SchoolClassId); // hoặc GetCourseGoalOrNull(...)
                var courseGoalConfig = courseGoal?.CourseGoalConfigs.FirstOrDefault(x => x.CourseId == cr.CourseId);

                if (courseGoal == null || courseGoalConfig == null)
                {
                    continue;
                }
                var totalTargetLessons = totalLessonsPerCourse.TryGetValue(cr.CourseId, out var total) ? total : 0;

                aggregates.Add(new StudentGoalAggregate
                {
                    StudentId = cr.StudentId,
                    CourseId = cr.CourseId,
                    CourseGoalId = courseGoal.Id,
                    CombinedProgress = EnumCombinedProgress.TotalBehindWeekBehind,
                    CurrentCombinedProgress = EnumCombinedProgress.TotalBehindWeekBehind,
                    CourseGoalConfigId = courseGoalConfig.Id,
                    SchoolId = courseGoal.SchoolId,
                    SchoolName = courseGoal.SchoolName,
                    ClassId = courseGoal.ClassId,
                    ClassName = courseGoal.ClassName,
                    TotalTargetLessons = totalTargetLessons,
                    CourseLevel = level,
                    IsActive = true,
                    CourseType = type,
                });
            }

            return aggregates;
        }

        private async Task EnsureWeeklySummariesForAggregatesAsync(
            IList<StudentGoalAggregate> aggregates,
            IList<CourseGoalModel> allCourseGoals,
            CancellationToken ct)
        {
            if (aggregates.Count == 0)
            {
                return;
            }
            var (weekStartUtc, weekEndUtc) = GetCurrentWeekRangeUtc();
            var toInsert = new List<StudentGoalSummary>();

            // Dựng lookup CourseGoal theo Id để khỏi FirstOrDefault nhiều lần
            var courseGoalById = allCourseGoals.ToDictionary(g => g.Id);

            foreach (var ag in aggregates)
            {
                var hasThisWeek = ag.StudentGoalSummaries?
                    .Any(s => s.StartDate >= weekStartUtc && s.EndDate <= weekEndUtc) == true;

                if (hasThisWeek)
                {
                    continue;
                }
                CourseGoalModel? courseGoal = courseGoalById.TryGetValue(ag.CourseGoalId, out var cg) ? cg : null;
                var cfg = courseGoal?.CourseGoalConfigs?.FirstOrDefault(c => c.Id == ag.CourseGoalConfigId);
                if (cfg == null)
                {
                    continue;
                }
                toInsert.Add(new StudentGoalSummary
                {
                    ProgressStatus = EnumProgressStatus.Behind,
                    StartDate = weekStartUtc,
                    EndDate = weekEndUtc,
                    StudentGoalAggregateId = ag.Id,
                    LessonsPerWeek = cfg.LessonsPerWeek,
                    TotalTargetLessons = ag.TotalTargetLessons,
                    TotalCompletedLessons = ag.TotalCompletedLessons,
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
            CancellationToken ct)
        {
            if (aggregates.Count == 0)
            {
                return;
            }
            var (weekStartUtc, weekEndUtc) = GetCurrentWeekRangeUtc();
            var toInsert = new List<StudentGoalSummary>();
            var updatedAggregates = new List<StudentGoalAggregate>();
            // Dựng lookup CourseGoal theo Id để khỏi FirstOrDefault nhiều lần
            var courseGoalById = allCourseGoals.ToDictionary(g => g.Id);
            var courseStudents = aggregates.Select(a => new { a.CourseId, a.StudentId }).Distinct().ToList();

            var courseResults = await _courseResultRepository.Queryable
                                                            .WhereBulkContains(courseStudents, new[] { "CourseId", "StudentId" })
                                                            .AsNoTracking()
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

                var hasThisWeek = ag.StudentGoalSummaries?
                    .Any(s => s.StartDate >= weekStartUtc && s.EndDate <= weekEndUtc) == true;

                if (hasThisWeek)
                {
                    continue;
                }
                var courseGoal = GetCourseGoal(allCourseGoals, ag.CourseLevel, ag.CourseType, student.SchoolClassId); // hoặc GetCourseGoalOrNull(...)
                var courseGoalConfig = courseGoal?.CourseGoalConfigs.FirstOrDefault(x => x.CourseId == ag.CourseId);
                if (courseGoal == null || courseGoalConfig == null)
                {
                    continue;
                }

                toInsert.Add(new StudentGoalSummary
                {
                    ProgressStatus = EnumProgressStatus.Behind,
                    StartDate = weekStartUtc,
                    EndDate = weekEndUtc,
                    StudentGoalAggregateId = ag.Id,
                    LessonsPerWeek = courseGoalConfig.LessonsPerWeek,
                    TotalTargetLessons = ag.TotalTargetLessons,
                    TotalCompletedLessons = ag.TotalCompletedLessons,
                });

                if (ag.CourseGoalConfigId != courseGoalConfig.Id || ag.CourseGoalId != courseGoal.Id)
                {
                    ag.CourseGoalConfigId = courseGoalConfig.Id;
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

        private static (DateTime weekStartUtc, DateTime weekEndUtc) GetCurrentWeekRangeUtc()
        {
            var nowUtc = DateTime.UtcNow;
            var nowVn = nowUtc.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date;

            var startVn = GetWeekStartMonday(nowVn);
            var endVn = startVn.AddDays(6);
            return (startVn, endVn);
        }

        private static DateTime GetWeekStartMonday(DateTime date)
        {
            var day = (int)date.DayOfWeek; // Sunday=0 ... Monday=1 ... Saturday=6
            var offset = day == 0 ? -6 : 1 - day; // về thứ 2
            return date.AddDays(offset);
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

        private async Task<IList<CourseResult>> GetUnlinkedCourseResultsAsync(CancellationToken ct = default)
        {
            var query = from cr in _courseResultRepository.Queryable.Include(x => x.Course)
                        where cr.WorkingStatus != EnumWorkingStatus.NotWorking && cr.Status != EnumResultStatus.Done
                        join ag0 in _studentLearningGoalAggregateRepository.Queryable.AsNoTracking().Where(x => x.IsActive)
                            on new { cr.CourseId, cr.StudentId }
                            equals new { ag0.CourseId, ag0.StudentId } into agGroup
                        from ag in agGroup.DefaultIfEmpty()
                        where ag == null
                        select cr;

            return await query.AsNoTracking().ToListAsync(ct);
        }

        private async Task RebuildStudentLearningProgressAsync(IList<CourseGoalModel> courseGoals, CancellationToken ct = default)
        {
            var nowVn = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            // 1) Lấy cặp (Aggregate, WeeklySummary) đang active từ DB
            var aggAndWeekly = await FetchActiveAggWeeklyPairsAsync(ct);
            if (!aggAndWeekly.Any())
            {
                return;
            }
            var aggregates = aggAndWeekly.Select(p => p.Aggregate).Distinct().ToList();
            var weeklySummaries = aggAndWeekly.Select(p => p.Weekly).ToList();

            var studentIds = aggregates.Select(a => a.StudentId).Distinct().ToList();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            var students = studentResults.Content?.Result ?? new List<StudentModel>();

            // 2) Lấy "bản ghi tuần mới nhất" theo Aggregate
            var latestWeeklyByAggId = PickLatestWeeklyByAggregateId(weeklySummaries);

            // 4) Nạp dữ liệu phụ trợ
            var doneLessonResultsMap = await LoadDoneLessonResultsMapAsync(aggregates, ct);

            // 5) Tính toán
            foreach (var ag in aggregates)
            {
                var student = students.FirstOrDefault(x => x.Id == ag.StudentId);
                if (student == null)
                {
                    continue;
                }

                if (!latestWeeklyByAggId.TryGetValue(ag.Id, out var weekly) || weekly is null)
                {
                    continue;
                }
                var weeklies = weeklySummaries.Where(x => x.StudentGoalAggregateId == ag.Id).Where(x => x.EndDate <= nowVn).ToList();

                var level = ag.CourseLevel;
                var type = ag.CourseType;

                var results = doneLessonResultsMap.TryGetValue((ag.StudentId, ag.CourseId), out var r)
                    ? r
                    : Enumerable.Empty<LessonResult>();

                ApplyWeeklyAndTotalProgress(nowVn, weekly, ag, results);
                ag.CurrentCombinedProgress = EnumCombinedProgressHelper.GetCurrentCombineProgress(weeklies.Sum(x => x.CompletedLessons), weeklies.Sum(x => x.LessonsPerWeek), weekly.ProgressStatus);
                ag.CombinedProgress = EnumCombinedProgressHelper.GetCombineProgress(weeklies.Sum(x => x.CompletedLessons), weeklies.Sum(x => x.LessonsPerWeek));
                UpdateBehindStreak(ag, weeklies);
            }

            // 6) Lưu
            _studentLearningGoalSummaryRepository.UpdateList(latestWeeklyByAggId.Values);
            await _studentLearningGoalSummaryRepository.UnitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

            _studentLearningGoalAggregateRepository.UpdateList(aggregates);
            await _studentLearningGoalAggregateRepository.UnitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

            // 6) Tạo weekly summaries cho tuần hiện tại nếu thiếu
            await EnsureWeeklySummariesForAggregatesAsync(aggregates, students, courseGoals, ct);
        }

        private static void UpdateBehindStreak(StudentGoalAggregate agg, IReadOnlyList<StudentGoalSummary> weeklySummaries)
        {
            if (weeklySummaries == null || weeklySummaries.Count == 0)
            {
                agg.ConsecutiveBehindWeeks = 0;
                return;
            }

            // Sắp xếp theo mốc thời gian để chắc chắn tuần cuối là mới nhất.
            // Ưu tiên EndDate, fallback StartDate/CreatedDate.
            var ordered = weeklySummaries
                .Where(w => w != null)
                .OrderBy(w => w.EndDate != default ? w.EndDate
                         : w.StartDate != default ? w.StartDate
                         : w.CreatedDate)
                .ToList();

            var streak = 0;

            for (int i = ordered.Count - 1; i >= 0; i--)
            {
                var status = ordered[i].ProgressStatus;

                // Nếu cần bỏ qua các tuần "không đánh giá", có thể thay thế điều kiện dưới đây
                // bằng check cụ thể (vd: status == EnumProgressStatus.Unknown => break hoặc continue).
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

        private static Dictionary<Guid, StudentGoalSummary> PickLatestWeeklyByAggregateId(
            IEnumerable<StudentGoalSummary> weeklies)
        {
            return weeklies
                .GroupBy(s => s.StudentGoalAggregateId)
                .Select(g => g.OrderByDescending(x => x.CreatedDate).First())
                .ToDictionary(x => x.StudentGoalAggregateId, x => x);
        }

        private async Task<Dictionary<Guid, int>> LoadCourseLessonCountsAsync(
            IEnumerable<Guid> courseIds, CancellationToken ct)
        {
            var rows = await _courseUnitMockTestRepository.Queryable
                .AsNoTracking()
                .WhereBulkContains(courseIds, x => x.CourseId)
                .Select(x => new { x.CourseId, Count = x.Unit!.UnitLessons.Count })
                .ToListAsync(ct);

            return rows.GroupBy(x => x.CourseId)
                       .ToDictionary(g => g.Key, g => g.Sum(x => x.Count));
        }

        private async Task<Dictionary<(Guid StudentId, Guid CourseId), IEnumerable<LessonResult>>>
            LoadDoneLessonResultsMapAsync(IList<StudentGoalAggregate> aggregates, CancellationToken ct)
        {
            var studentCourseKeys = aggregates.Select(a => new
            {
                a.StudentId,
                a.CourseId
            }).Distinct().ToList();

            var rows = await _lessonResultRepository.Queryable
                .Where(x => x.Status == EnumResultStatus.Done)
                .WhereBulkContains(studentCourseKeys, new[] { "StudentId", "CourseId" })
                .Select(x => new LessonResult
                {
                    StudentId = x.StudentId,
                    CourseId = x.CourseId,
                    CompletionDate = x.CompletionDate,
                    UpdatedDate = x.UpdatedDate
                })
                .ToListAsync(ct);

            return rows.GroupBy(x => (x.StudentId, x.CourseId))
                       .ToDictionary(g => g.Key, g => g.AsEnumerable());
        }

        private static void ApplyWeeklyAndTotalProgress(
            DateTime nowVn,
            StudentGoalSummary weekly,
            StudentGoalAggregate agg,
            IEnumerable<LessonResult> results)
        {
            // Lần hoàn thành gần nhất
            var last = results.OrderByDescending(x => x.CompletionDate ?? x.UpdatedDate).FirstOrDefault();
            weekly.LastCompletedAt = last?.CompletionDate ?? last?.UpdatedDate;

            // Tuần hiện tại (từ CreatedDate weekly → now)
            var start = weekly.CreatedDate;
            var end = nowVn;

            var completedThisWeek = results.Count(x =>
                (x.CompletionDate ?? x.UpdatedDate) >= start &&
                (x.CompletionDate ?? x.UpdatedDate) <= end);

            weekly.CompletedLessons = completedThisWeek;
            weekly.ProgressStatus = EnumCombinedProgressHelper.GetProgressStatusFromCounts(completedThisWeek, weekly.LessonsPerWeek);

            // Tổng
            var totalCompleted = results.Count();
            weekly.TotalCompletedLessons = totalCompleted;
            agg.TotalCompletedLessons = totalCompleted;
        }
    }
}

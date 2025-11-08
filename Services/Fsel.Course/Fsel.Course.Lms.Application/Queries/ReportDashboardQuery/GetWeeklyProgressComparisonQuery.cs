// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReportDashboardQuery
{
    using System.Linq;
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.BaseChartModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetWeeklyProgressComparisonQuery : IRequest<MethodResult<StackBarChartsModel>>
    {
        public Guid SchoolId { get; set; }
        public Guid? ClassIdStr { get; set; }
        public EnumCourseType? CourseTypeStr { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GetWeeklyProgressComparisonQueryHandler : IRequestHandler<GetWeeklyProgressComparisonQuery, MethodResult<StackBarChartsModel>>
    {
        private readonly IStudentGoalAggregateRepository _studentGoalAggregateRepository;
        private readonly IStudentGoalSummaryRepository _studentGoalSummaryRepository;

        public GetWeeklyProgressComparisonQueryHandler(
            IStudentGoalAggregateRepository studentGoalAggregateRepository,
            IStudentGoalSummaryRepository studentGoalSummaryRepository)
        {
            _studentGoalAggregateRepository = studentGoalAggregateRepository;
            _studentGoalSummaryRepository = studentGoalSummaryRepository;
        }

        public async Task<MethodResult<StackBarChartsModel>> Handle(GetWeeklyProgressComparisonQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<StackBarChartsModel>();

            // Chuẩn hóa "now" theo VN (cho cả chart + summary)
            var nowUtc = request.EndDate ?? DateTime.UtcNow;
            var nowVn = nowUtc.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date;

            // Các tuần cover full month của nowVn để build chart
            var (weeksInMonth, startDate, endDate) = GetWeeksCoveringMonth(nowVn);

            // Lấy data trong khoảng tuần của tháng (tối ưu: query 1 lần)
            var (goalSummaries, startDateGoal) = await GetGoalSummariesAsync(
                request,
                startDate,
                endDate,
                nowVn,
                cancellationToken);

            // Build dữ liệu cho toàn bộ tuần trong tháng
            var points = BuildWeeklyPoints(goalSummaries, weeksInMonth);

            // Xác định tuần hiện tại và tuần trước (2 tuần để tính summary)
            var (currentWeek, previousWeek) = GetCurrentAndPreviousWeeksFromNow(nowVn);

            // Tính summary dựa trên goalSummaries + 2 tuần
            var summary = BuildWeeklySummary(goalSummaries, currentWeek, previousWeek);

            methodResult.Result = new StackBarChartsModel
            {
                Type = EnumChartType.BarChart,
                DataCharts = points,
                StartDate = startDateGoal,
                Summary = summary
            };

            return methodResult;
        }

        #region Weeks helpers

        private static (List<(DateTime start, DateTime end)> weeks, DateTime startDate, DateTime endDate)
            GetWeeksCoveringMonth(DateTime nowVn)
        {
            static DateTime GetMonday(DateTime d)
            {
                int dow = (int)d.DayOfWeek; // Sunday=0, Monday=1, ...
                int offset = dow == 0 ? -6 : 1 - dow; // lùi về thứ 2
                return d.AddDays(offset).Date;
            }

            static DateTime GetSunday(DateTime d)
            {
                var mon = GetMonday(d);
                return mon.AddDays(6).Date;
            }

            var firstDay = new DateTime(nowVn.Year, nowVn.Month, 1);
            var lastDay = new DateTime(nowVn.Year, nowVn.Month, DateTime.DaysInMonth(nowVn.Year, nowVn.Month));

            var startOfFirstWeek = GetMonday(firstDay);
            var endOfLastWeek = GetSunday(lastDay);

            var weeks = new List<(DateTime start, DateTime end)>();
            var curStart = startOfFirstWeek;

            while (curStart <= endOfLastWeek)
            {
                var curEnd = curStart.AddDays(6).Date;
                weeks.Add((curStart.Date, curEnd));
                curStart = curStart.AddDays(7);
            }

            return (weeks, weeks.First().start, weeks.Last().end);
        }

        private static (
            (DateTime start, DateTime end)? currentWeek,
            (DateTime start, DateTime end)? previousWeek)
            GetCurrentAndPreviousWeeksFromNow(DateTime nowVn)
        {
            static DateTime GetMonday(DateTime d)
            {
                int dow = (int)d.DayOfWeek;
                int offset = dow == 0 ? -6 : 1 - dow;
                return d.AddDays(offset).Date;
            }

            var currentWeekStart = GetMonday(nowVn);
            var currentWeekEnd = currentWeekStart.AddDays(6);

            var previousWeekStart = currentWeekStart.AddDays(-7);
            var previousWeekEnd = previousWeekStart.AddDays(6);

            return (
                (currentWeekStart, currentWeekEnd),
                (previousWeekStart, previousWeekEnd));
        }

        #endregion Weeks helpers

        #region Data query

        private async Task<(List<StudentGoalSummary> summaries, DateTime? firstGoalStartDate)>
            GetGoalSummariesAsync(
                GetWeeklyProgressComparisonQuery request,
                DateTime startDate,
                DateTime endDate,
                DateTime nowVn,
                CancellationToken cancellationToken)
        {
            var query = _studentGoalAggregateRepository.Queryable
                .Where(x => x.IsActive);

            if (request.ClassIdStr.HasValue)
            {
                query = query.Where(x => x.ClassId == request.ClassIdStr);
            }

            if (request.CourseTypeStr.HasValue)
            {
                query = query.Where(x => x.CourseType == request.CourseTypeStr);
            }

            // Lấy mốc bắt đầu goal sớm nhất
            var firstGoalStartDate = await (
                    from baseQ in query
                    join sgs in _studentGoalSummaryRepository.Queryable
                        on baseQ.Id equals sgs.StudentGoalAggregateId
                    where baseQ.SchoolId == request.SchoolId
                    orderby sgs.StartDate
                    select sgs.StartDate
                ).FirstOrDefaultAsync(cancellationToken);

            // Nếu chưa có dữ liệu thì trả luôn rỗng
            if (firstGoalStartDate == default)
            {
                return (new List<StudentGoalSummary>(), null);
            }

            // Nếu now trước thời điểm có goal thì không cần load thêm
            if (nowVn < firstGoalStartDate.Date)
            {
                return (new List<StudentGoalSummary>(), firstGoalStartDate);
            }

            // Load summaries trong khoảng tuần để dùng cho cả chart + summary
            var summaries = await (
                    from baseQ in query
                    join sgs in _studentGoalSummaryRepository.Queryable.AsNoTracking()
                        on baseQ.Id equals sgs.StudentGoalAggregateId
                    where baseQ.SchoolId == request.SchoolId
                          && sgs.StartDate.Date >= startDate
                          && sgs.EndDate.Date <= endDate
                    select new StudentGoalSummary
                    {
                        StartDate = sgs.StartDate,
                        EndDate = sgs.EndDate,
                        ProgressStatus = sgs.ProgressStatus
                    }
                ).ToListAsync(cancellationToken);

            return (summaries, firstGoalStartDate);
        }

        #endregion Data query

        #region Build chart + summary

        private static List<StackBarChartModel> BuildWeeklyPoints(
            IEnumerable<StudentGoalSummary> data,
            IEnumerable<(DateTime start, DateTime end)> weeks)
        {
            int CountBehind((DateTime start, DateTime end) w) =>
                data.Count(x =>
                    x.ProgressStatus == EnumProgressStatus.Behind &&
                    x.StartDate <= w.end &&
                    x.EndDate >= w.start);

            return weeks.Select(w =>
            {
                var count = CountBehind(w);
                return new StackBarChartModel
                {
                    WeekStart = w.start,
                    WeekEnd = w.end,
                    Label = $"{w.start:dd/MM}–{w.end:dd/MM}",
                    NumericValue = count,
                    Value = count.ToString()
                };
            }).ToList();
        }

        private static WeekSummary BuildWeeklySummary(
            IEnumerable<StudentGoalSummary> data,
            (DateTime start, DateTime end)? currentWeek,
            (DateTime start, DateTime end)? previousWeek)
        {
            var summary = new WeekSummary();

            if (data == null || !data.Any() || currentWeek == null)
            {
                return summary;
            }

            int CountBehind((DateTime start, DateTime end) w) =>
                data.Count(x =>
                    x.ProgressStatus == EnumProgressStatus.Behind &&
                    x.StartDate <= w.end &&
                    x.EndDate >= w.start);

            var current = CountBehind(currentWeek.Value);
            var prev = previousWeek.HasValue ? CountBehind(previousWeek.Value) : 0;

            summary.CurrentBehind = current;
            summary.PrevBehind = prev;
            summary.ChangeAbs = Math.Abs(current - prev);
            summary.ChangePercent = (int)NumberHelper.GetPercentChart(current - prev, prev);

            return summary;
        }

        #endregion Build chart + summary
    }
}

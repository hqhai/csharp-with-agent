// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReportDashboardQuery
{
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

        public GetWeeklyProgressComparisonQueryHandler(IStudentGoalAggregateRepository studentGoalAggregateRepository,
            IStudentGoalSummaryRepository studentGoalSummaryRepository)
        {
            _studentGoalAggregateRepository = studentGoalAggregateRepository;
            _studentGoalSummaryRepository = studentGoalSummaryRepository;
        }

        public async Task<MethodResult<StackBarChartsModel>> Handle(GetWeeklyProgressComparisonQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<StackBarChartsModel>();

            var (weeks, startDate, endDate, now) = GetWeeksCoveringMonth(request);

            var (data, startDateGoal) = await GetGoalSummariesAsync(request, startDate, endDate, now, cancellationToken);

            var points = BuildWeeklyPoints(data, weeks);
            var summary = BuildWeeklySummary(points, now);
            methodResult.Result = new StackBarChartsModel
            {
                Type = EnumChartType.BarChart,
                DataCharts = points,
                StartDate = startDateGoal,
                Summary = summary
            };

            return methodResult;
        }

        private static (List<(DateTime start, DateTime end)> weeks, DateTime startDate, DateTime endDate, DateTime now) GetWeeksCoveringMonth(GetWeeklyProgressComparisonQuery request)
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
            // Ngày đầu và cuối tháng
            var nowUtc = request.EndDate ?? DateTime.UtcNow;
            var nowVn = nowUtc.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var firstDay = new DateTime(nowVn.Year, nowVn.Month, 1);
            var lastDay = new DateTime(nowVn.Year, nowVn.Month, DateTime.DaysInMonth(nowVn.Year, nowVn.Month));

            // Tìm thứ Hai đầu tiên trước hoặc bằng ngày đầu tháng
            var startOfFirstWeek = GetMonday(firstDay);
            // Tìm Chủ nhật cuối cùng sau hoặc bằng ngày cuối tháng
            var endOfLastWeek = GetSunday(lastDay);

            // Lặp từ startOfFirstWeek → endOfLastWeek
            var weeks = new List<(DateTime start, DateTime end)>();
            var curStart = startOfFirstWeek;

            while (curStart <= endOfLastWeek)
            {
                var curEnd = curStart.AddDays(6);
                weeks.Add((curStart.Date, curEnd.Date));
                curStart = curStart.AddDays(7);
            }

            return (weeks, weeks.First().Item1.Date, weeks.Last().Item2.Date, nowVn);
        }

        private async Task<(List<StudentGoalSummary>, DateTime?)> GetGoalSummariesAsync(
            GetWeeklyProgressComparisonQuery request,
            DateTime startDate,
            DateTime endDate,
            DateTime nowVn,
            CancellationToken cancellationToken)
        {
            var query = _studentGoalAggregateRepository.Queryable;

            if (request.ClassIdStr.HasValue)
            {
                query = query.Where(x => x.ClassId == request.ClassIdStr);
            }

            if (request.CourseTypeStr.HasValue)
            {
                query = query.Where(x => x.CourseType == request.CourseTypeStr);
            }
            var firstGoalStartDate = await (from baseQ in query
                                            join sgs in _studentGoalSummaryRepository.Queryable
                                              on baseQ.Id equals sgs.StudentGoalAggregateId
                                            where baseQ.SchoolId == request.SchoolId
                                            orderby sgs.StartDate
                                            select sgs.StartDate)
                                        .FirstOrDefaultAsync(cancellationToken);
            if ((nowVn.Month < firstGoalStartDate.Month && nowVn.Year <= firstGoalStartDate.Year) || nowVn.Year < firstGoalStartDate.Year)
            {
                return (new List<StudentGoalSummary>(), firstGoalStartDate);
            }

            var studentGoals = await (from baseQ in query
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
                                      }).ToListAsync(cancellationToken);

            return (studentGoals, firstGoalStartDate);
        }

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
            List<StackBarChartModel> points,
            DateTime now)
        {
            var currentPoint = points.FirstOrDefault(p => p.WeekStart <= now && p.WeekEnd >= now);
            var prevPoint = points
                .Where(p => p.WeekEnd < now)
                .OrderByDescending(p => p.WeekEnd)
                .FirstOrDefault();

            var current = currentPoint?.NumericValue ?? 0;
            var prev = prevPoint?.NumericValue ?? 0;

            return new WeekSummary
            {
                CurrentBehind = current,
                PrevBehind = prev,
                ChangeAbs = current - prev,
                ChangePercent = (int)NumberHelper.GetPercentChart(current, prev)
            };
        }
    }
}

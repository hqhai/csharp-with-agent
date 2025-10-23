// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReportDashboardQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.BaseChartModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetWeeklyProgressComparisonQuery : IRequest<MethodResult<StackBarChartsModel>>
    {
        public Guid SchoolId { get; set; }
        public string? ClassIdStr { get; set; }
        public string? CourseTypeStr { get; set; }
        public DateTime? EndDate { get; set; }
        public int WindowWeeks { get; set; } = 4;
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

            var classIds = request.ClassIdStr.ToList<Guid>();
            var courseTypes = request.CourseTypeStr.ToList<Guid>();

            static DateTime GetWeekStartMonday(DateTime date)
            {
                var day = (int)date.DayOfWeek; // Sunday=0 ... Monday=1 ... Saturday=6
                var offset = day == 0 ? -6 : 1 - day; // về thứ 2
                return date.AddDays(offset);
            }
            static (DateTime start, DateTime end) WeekRange(DateTime anyInWeek)
            {
                var start = GetWeekStartMonday(anyInWeek);
                var end = start.AddDays(6);
                return (start, end);
            }

            var endDate = request.EndDate ?? DateTime.UtcNow;

            var weeks = new List<(DateTime start, DateTime end)>();
            {
                var cur = WeekRange(endDate).end;
                for (int i = request.WindowWeeks - 1; i >= 0; i--)
                {
                    var wEnd = WeekRange(cur.AddDays(-7 * i)).end;
                    var wStart = GetWeekStartMonday(wEnd);
                    weeks.Add((wStart, wEnd));
                }
            }
            var startDate = weeks.FirstOrDefault().start.Date;
            var lastDate = weeks.LastOrDefault().end.Date;

            var query = _studentGoalAggregateRepository.Queryable;
            if (!string.IsNullOrEmpty(request.ClassIdStr))
            {
                query = query.WhereBulkContains(classIds, x => x.ClassId);
            }
            if (!string.IsNullOrEmpty(request.CourseTypeStr))
            {
                query = query.WhereBulkContains(courseTypes, x => x.CourseType);
            }

            var data = await (from baseQ in query
                              join sgs in _studentGoalSummaryRepository.Queryable on baseQ.Id equals sgs.StudentGoalAggregateId
                              where baseQ.SchoolId == request.SchoolId
                              && sgs.StartDate.Date >= startDate
                              && sgs.EndDate.Date <= endDate
                              select new
                              {
                                  SchoolId = baseQ.SchoolId,
                                  ClassId = baseQ.ClassId,
                                  SchoolName = baseQ.SchoolName,
                                  StartDate = sgs.StartDate,
                                  EndDate = sgs.EndDate,
                                  ClassName = baseQ.ClassName,
                                  ProgressStatus = sgs.ProgressStatus,
                                  StudentId = baseQ.StudentId
                              }).ToListAsync(cancellationToken);

            int CountOnTrackInWeek((DateTime start, DateTime end) w) =>
            data.Count(x =>
                x.ProgressStatus == EnumProgressStatus.OnTrack &&
                x.StartDate <= w.end &&
                x.EndDate >= w.start);

            var points = weeks.Select(w =>
            {
                var count = CountOnTrackInWeek(w);
                return new StackBarChartModel
                {
                    WeekStart = w.start,
                    WeekEnd = w.end,
                    Label = $"{w.start:dd/MM}–{w.end:dd/MM}",
                    Value = $"{count}",
                    NumericValue = count
                };
            }).ToList();

            // ---- Summary (tuần hiện tại vs tuần trước) ----
            var current = points.LastOrDefault()?.NumericValue ?? 0;
            var prev = points.Count >= 2 ? points[^2].NumericValue : 0;

            var weekly = new StackBarChartsModel
            {
                Type = EnumChartType.BarChart,  // bar đơn (không stacked)
                DataCharts = points,
                Summary = new WeekSummary
                {
                    CurrentOnTrack = current,
                    PrevOnTrack = prev,
                    ChangeAbs = current - prev,
                    ChangePercent = (int)NumberHelper.GetPercent(current, prev)
                }
            };

            methodResult.Result = weekly;
            return methodResult;
        }
    }
}

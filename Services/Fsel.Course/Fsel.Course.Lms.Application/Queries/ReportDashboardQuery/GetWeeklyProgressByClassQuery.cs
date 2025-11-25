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

    public class GetWeeklyProgressByClassQuery : IRequest<MethodResult<IList<StackBarChartsModel>>>
    {
        public Guid SchoolId { get; set; }
        public Guid? ClassIdStr { get; set; }
        public EnumCourseType? CourseTypeStr { get; set; }
    }

    public class GetWeeklyProgressByClassQueryHandler : IRequestHandler<GetWeeklyProgressByClassQuery, MethodResult<IList<StackBarChartsModel>>>
    {
        private readonly IStudentGoalAggregateRepository _studentGoalAggregateRepository;
        private readonly IStudentGoalSummaryRepository _studentGoalSummaryRepository;

        public GetWeeklyProgressByClassQueryHandler(IStudentGoalAggregateRepository studentGoalAggregateRepository,
            IStudentGoalSummaryRepository studentGoalSummaryRepository)
        {
            _studentGoalAggregateRepository = studentGoalAggregateRepository;
            _studentGoalSummaryRepository = studentGoalSummaryRepository;
        }

        public async Task<MethodResult<IList<StackBarChartsModel>>> Handle(GetWeeklyProgressByClassQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StackBarChartsModel>>();

            var (currentWeekStartUtc, previousWeekStartUtc) = GetWeekBoundariesUtc();

            var query = BuildBaseQuery(request);
            var data = await GetDataForWeekAsync(query, request.SchoolId, previousWeekStartUtc, cancellationToken)
                     ?? await GetDataForWeekAsync(query, request.SchoolId, currentWeekStartUtc, cancellationToken)
                     ?? new List<ProgressRow>();
            var dataPre = await GetDataForWeekAsync(query, request.SchoolId, previousWeekStartUtc, cancellationToken) ?? new List<ProgressRow>();
            var stackBar = BuildStackBarChart(data);
            var pie = BuildPieChart(data, dataPre);

            methodResult.Result = new List<StackBarChartsModel> { stackBar, pie };
            return methodResult;
        }

        private static StackBarChartsModel BuildStackBarChart(List<ProgressRow> data)
        {
            var grouped = data
                .GroupBy(x => new { x.ClassId, x.ClassName })
                .Select(g => new
                {
                    g.Key.ClassId,
                    g.Key.ClassName,
                    OnTrack = g.Count(s => s.ProgressStatus == EnumProgressStatus.OnTrack || s.ProgressStatus == EnumProgressStatus.Ahead),
                    Behind = g.Count(s => s.ProgressStatus == EnumProgressStatus.Behind)
                })
                .OrderByDescending(x => x.OnTrack + x.Behind)
                .ThenBy(x => x.ClassName)
                .ToList();

            var models = grouped.Select(g => new StackBarChartModel
            {
                Label = g.ClassName,
                Value = g.ClassId.ToString(),
                DataColumns = new List<DataChartModel>
                {
                    new DataChartModel { Label = EnumProgressStatus.OnTrack.GetDescription(), Value = g.OnTrack },
                    new DataChartModel { Label = EnumProgressStatus.Behind.GetDescription(),  Value = g.Behind  }
                }
            }).ToList();

            return new StackBarChartsModel
            {
                Type = EnumChartType.StackbarChart,
                DataCharts = models
            };
        }

        private static StackBarChartsModel BuildPieChart(List<ProgressRow> data, List<ProgressRow> dataPre)
        {
            var totalOnTrack = data.Count(s => s.ProgressStatus == EnumProgressStatus.OnTrack || s.ProgressStatus == EnumProgressStatus.Ahead);
            var totalBehind = data.Count(s => s.ProgressStatus == EnumProgressStatus.Behind);
            var grandTotal = totalOnTrack + totalBehind;

            var totalOnTrackPre = dataPre.Count(s => s.ProgressStatus == EnumProgressStatus.OnTrack || s.ProgressStatus == EnumProgressStatus.Ahead);
            var totalBehindPre = dataPre.Count(s => s.ProgressStatus == EnumProgressStatus.Behind);

            var onTrackDelta = totalOnTrack - totalOnTrackPre;
            var behindDelta = totalBehind - totalBehindPre;

            return new StackBarChartsModel
            {
                Type = EnumChartType.PieChart,
                DataCharts = new List<StackBarChartModel>
                {
                    new StackBarChartModel
                    {
                        Label = EnumProgressStatus.OnTrack.GetDescription(),
                        Value = totalOnTrack.ToString(),
                        NumericValue = totalOnTrack,
                        DataColumns = new List<DataChartModel>
                        {
                            new DataChartModel
                            {
                                Label = onTrackDelta < 0?  EnumProgressStatus.Behind.ToString() : EnumProgressStatus.OnTrack.ToString(),
                                Value = (int)NumberHelper.GetPercentChart(onTrackDelta, totalOnTrackPre)
                            }
                        },
                        Percent = (int)NumberHelper.GetPercent(totalOnTrack, grandTotal)
                    },
                    new StackBarChartModel
                    {
                        Label = EnumProgressStatus.Behind.GetDescription(),
                        Value = totalBehind.ToString(),
                        NumericValue = totalBehind,
                        DataColumns = new List<DataChartModel>
                        {
                            new DataChartModel
                            {
                                Label = behindDelta < 0?  EnumProgressStatus.Behind.ToString() : EnumProgressStatus.OnTrack.ToString(),
                                Value = (int)NumberHelper.GetPercentChart(behindDelta, totalBehindPre)
                            }
                        },
                        Percent = (int)NumberHelper.GetPercent(totalBehind, grandTotal)
                    }
                },
            };
        }

        private static (DateTime currentWeekStartUtc, DateTime previousWeekStartUtc) GetWeekBoundariesUtc()
        {
            var nowUtc = DateTime.UtcNow.Date;
            int delta = ((int)nowUtc.DayOfWeek + 6) % 7; // Monday=0
            var currentWeekStartUtc = nowUtc.AddDays(-delta);
            var previousWeekStartUtc = currentWeekStartUtc.AddDays(-7);
            return (currentWeekStartUtc, previousWeekStartUtc);
        }

        private IQueryable<StudentGoalAggregate> BuildBaseQuery(GetWeeklyProgressByClassQuery request)
        {
            var query = _studentGoalAggregateRepository.Queryable.Where(x => x.IsActive).AsNoTracking();

            if (request.ClassIdStr.HasValue)
            {
                query = query.Where(x => x.ClassId == request.ClassIdStr.Value);
            }

            if (request.CourseTypeStr.HasValue)
            {
                query = query.Where(x => x.CourseType == request.CourseTypeStr.Value);
            }
            return query;
        }

        private async Task<List<ProgressRow>?> GetDataForWeekAsync(
            IQueryable<StudentGoalAggregate> query,
            Guid schoolId,
            DateTime weekStartUtc,
            CancellationToken ct)
        {
            var data = await (
                from baseQ in query
                join sgs in _studentGoalSummaryRepository.Queryable.AsNoTracking()
                    on baseQ.Id equals sgs.StudentGoalAggregateId
                where baseQ.SchoolId == schoolId
                      && sgs.StartDate <= weekStartUtc
                      && sgs.EndDate >= weekStartUtc
                select new ProgressRow
                {
                    ClassId = baseQ.ClassId,
                    ClassName = baseQ.ClassName,
                    ProgressStatus = sgs.ProgressStatus
                }).ToListAsync(ct);

            return data.Any() ? data : null;
        }

        private class ProgressRow
        {
            public Guid? ClassId { get; set; }
            public string? ClassName { get; set; }
            public EnumProgressStatus ProgressStatus { get; set; }
        }
    }
}

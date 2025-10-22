// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReportDashboardQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.BaseChartModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetWeeklyProgressByClassQuery : IRequest<MethodResult<IList<StackBarChartsModel>>>
    {
        public Guid SchoolId { get; set; }
        public string? ClassIdStr { get; set; }
        public string? CourseTypeStr { get; set; }
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

            var classIds = request.ClassIdStr.ToList<Guid>();
            var courseTypes = request.CourseTypeStr.ToList<EnumCourseType>();

            var nowUtc = DateTime.UtcNow;
            int diff = ((int)nowUtc.DayOfWeek + 6) % 7;
            var weekStartUtc = nowUtc.Date.AddDays(-diff - 7).Date;

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
                              && sgs.StartDate.Date <= weekStartUtc && sgs.EndDate.Date >= weekStartUtc
                              select new
                              {
                                  SchoolId = baseQ.SchoolId,
                                  ClassId = baseQ.ClassId,
                                  SchoolName = baseQ.SchoolName,
                                  ClassName = baseQ.ClassName,
                                  ProgressStatus = sgs.ProgressStatus,
                                  StudentId = baseQ.StudentId
                              }).ToListAsync(cancellationToken);
            var stackBar = new StackBarChartsModel
            {
                Type = EnumChartType.StackbarChart
            };
            stackBar.DataCharts = data
                            .GroupBy(x => new { x.ClassId, x.ClassName })
                            .OrderBy(g => g.Count()) // tuỳ bạn sort theo tên hoặc mã lớp
                            .Select(g =>
                            {
                                var onTrack = g.Count(s => s.ProgressStatus == EnumProgressStatus.OnTrack || s.ProgressStatus == EnumProgressStatus.Ahead);
                                var behind = g.Count(s => s.ProgressStatus == EnumProgressStatus.Behind);
                                return new StackBarChartModel
                                {
                                    Label = g.Key.ClassName,
                                    Value = g.Key.ClassId.ToString(),
                                    DataColumns = new List<DataChartModel>
                                    {
                                        new DataChartModel { Label = EnumProgressStatus.OnTrack.GetDescription(),  Value = onTrack },
                                        new DataChartModel { Label = EnumProgressStatus.Behind.GetDescription(), Value = behind  }
                                    }
                                };
                            })
                            .ToList();
            var totalOnTrack = data.Count(s => s.ProgressStatus == EnumProgressStatus.OnTrack || s.ProgressStatus == EnumProgressStatus.Ahead);
            var totalBehind = data.Count(s => s.ProgressStatus == EnumProgressStatus.Behind);
            var grandTotal = totalOnTrack + totalBehind;
            var pie = new StackBarChartsModel
            {
                Type = EnumChartType.PieChart,
                DataCharts = new List<StackBarChartModel>
                {
                    new StackBarChartModel
                    {
                        Label = EnumProgressStatus.OnTrack.GetDescription(),
                        Value =$"{totalOnTrack}",
                        Percent = (int)NumberHelper.GetPercent(totalOnTrack, grandTotal)
                    },
                    new StackBarChartModel
                    {
                        Label = EnumProgressStatus.Behind.GetDescription(),
                        Value = $"{totalBehind}",
                        Percent = (int)NumberHelper.GetPercent(totalBehind, grandTotal)
                    }
                }
            };

            // ----- Tuỳ chọn: gộp vào DTO trả về cho API -----
            methodResult.Result = new List<StackBarChartsModel> { stackBar, pie };
            return methodResult;
        }
    }
}

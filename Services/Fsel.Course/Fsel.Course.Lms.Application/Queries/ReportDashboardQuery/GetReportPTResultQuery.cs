// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReportDashboardQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.QueryModels.ReportDashboard;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;

    public class GetReportPTResultQuery : IRequest<MethodResult<ReportPTResultModel>>
    {
        public Guid SchoolId { get; set; }
    }

    public class GetReportPTResultQueryHandler : IRequestHandler<GetReportPTResultQuery, MethodResult<ReportPTResultModel>>
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;
        public GetReportPTResultQueryHandler(IMapper mapper, IUserService userService, IPlacementTestGroupResultRepository placementTestGroupResultRepository)
        {
            _mapper = mapper;
            _userService = userService;
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
        }

        public async Task<MethodResult<ReportPTResultModel>> Handle(GetReportPTResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ReportPTResultModel>();

            ReportPTResultModel reportPTResult = new ReportPTResultModel();
            var studentResult = await _userService.GetStudentsBySchoolId(request.SchoolId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student.Count <= 0)
            {
                methodResult.Result = reportPTResult;
                return methodResult;
            }
            int totalStudents = student.Count;
            var studentIds = student.Select(s => s.Id).ToList();
            int numberStudentFinishPT = _placementTestGroupResultRepository.Queryable
                .Where(p => studentIds.Contains(p.StudentId) && p.Status == Domain.Enums.EnumResultStatus.Done)
                .Select(p => p.StudentId)
                .Distinct()
                .Count();

            int numberStudentNotFinishPT = totalStudents - numberStudentFinishPT;
            int percentStudentFinishPT = (int)(Math.Ceiling((double)numberStudentFinishPT / totalStudents * 100));
            int percentStudentNotFinishPT = 100 - percentStudentFinishPT;

            reportPTResult.OverallStatic.TotalStudentInSchool = student.Count;
            reportPTResult.OverallStatic.NumberStudentFinishPT = numberStudentFinishPT;
            reportPTResult.OverallStatic.NumberStudentNotFinishPT = numberStudentNotFinishPT;

            if (student != null)
            {
                BaseChartResult overallEvaluation = new BaseChartResult
                {
                    Type = EnumChartType.PieChart,
                    DataCharts = new List<DataChart>
                    {
                        new DataChart
                        {
                            Label = ChartConstant.PercentStudentFinishPT,
                            Value = percentStudentFinishPT
                        },
                        new DataChart
                        {
                            Label = ChartConstant.PercentStudentNotFinishPT,
                            Value = percentStudentNotFinishPT
                        }
                    }
                };

                BaseChartResult amountStudentByLevel = new BaseChartResult
                {
                    Type = EnumChartType.BarChart,
                    DataCharts = student.GroupBy(student => student.CourseLevel)
                        .OrderBy(group => group.Key)
                        .Select(group => new DataChart
                        {
                            Label = group.Key.ToString(),
                            Value = group.Count()
                        })
                        .ToList()
                };

                var amountStudentByLevelAndClass = student.Where(student => student.SchoolId == request.SchoolId)
                                                          .GroupBy(student => new { student.SchoolClass, student.CourseLevel })
                                                          .Select(group => new
                                                          {
                                                              ClassName = group.Key.SchoolClass,
                                                              CourseLevel = group.Key.CourseLevel,
                                                              Count = group.Count()
                                                          })
                                                          .GroupBy(x => x.ClassName)
                                                          .OrderBy(classGroup => classGroup.Key)
                                                          .Select(classGroup => new StackBarChart
                                                          {
                                                              Labels = classGroup.Key,
                                                              DataColumns = classGroup.OrderBy(levelGroup => levelGroup.CourseLevel)
                                                                                      .Select(levelGroup => new DataChart
                                                                                      {
                                                                                          Label = levelGroup.CourseLevel.ToString(),
                                                                                          Value = levelGroup.Count
                                                                                      }).ToList()
                                                          })
                                                          .ToList();
                StackBarCharts stackBarCharts = new StackBarCharts
                {
                    Type = EnumChartType.StackbarChart,
                    DataCharts = amountStudentByLevelAndClass
                };

                reportPTResult.OverallEvaluation = overallEvaluation;
                reportPTResult.AmountStudentByLevel = amountStudentByLevel;
                reportPTResult.AmountStudentByLevelAndClass = stackBarCharts;
            }
            methodResult.Result = reportPTResult;
            return methodResult;
        }
    }

}

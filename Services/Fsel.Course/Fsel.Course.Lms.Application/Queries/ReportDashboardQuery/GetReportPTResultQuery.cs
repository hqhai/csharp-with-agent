// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReportDashboardQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.BaseChartModels;
    using Fsel.Course.Domain.Models.QueryModels.ReportDashboard;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;

    public class GetReportPTResultQuery : IRequest<MethodResult<ReportPTResultModel>>
    {
        public IList<string> ListClassName { get; set; } = new List<string>();
        public DateTime? ToDate { get; set; }
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
            var studentResult = await _userService.GetStudentsBySchoolId();
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student?.Count <= 0)
            {
                methodResult.Result = reportPTResult;
                return methodResult;
            }
            var filteredStudents = (from s in student
                                    join p in _placementTestGroupResultRepository.Queryable
                                    on s.Id equals p.StudentId
                                    where p.Status == Domain.Enums.EnumResultStatus.Done &&
                                          (!request.ToDate.HasValue || p.CompletionDate <= request.ToDate.Value)
                                    select new
                                    {
                                        Student = s,
                                        PlacementResult = p
                                    }).ToList();

            int totalStudents = student.Count;
            int numberStudentFinishPT = filteredStudents.Count;
            int numberStudentNotFinishPT = totalStudents - numberStudentFinishPT;
            int percentStudentFinishPT = (int)(Math.Ceiling((double)numberStudentFinishPT / totalStudents * 100));
            int percentStudentNotFinishPT = 100 - percentStudentFinishPT;

            reportPTResult.OverallStatic.TotalStudentInSchool = totalStudents;
            reportPTResult.OverallStatic.NumberStudentFinishPT = numberStudentFinishPT;
            reportPTResult.OverallStatic.NumberStudentNotFinishPT = numberStudentNotFinishPT;
            reportPTResult.SchoolName = filteredStudents.FirstOrDefault()?.Student.School;

            if (student != null)
            {
                BaseChartResultModel overallEvaluation = new BaseChartResultModel
                {
                    Type = EnumChartType.PieChart,
                    DataCharts = new List<DataChartModel>
                    {
                        new DataChartModel
                        {
                            Label = ChartConstant.PercentStudentFinishPT,
                            Value = percentStudentFinishPT
                        },
                        new DataChartModel
                        {
                            Label = ChartConstant.PercentStudentNotFinishPT,
                            Value = percentStudentNotFinishPT
                        }
                    }
                };

                BaseChartResultModel amountStudentByLevel = new BaseChartResultModel
                {
                    Type = EnumChartType.BarChart,
                    DataCharts = filteredStudents.Where(student => student.PlacementResult.ProcessLevel != EnumPlacementTestLevel.IELTS &&
                                                                   student.PlacementResult.CurrentLevel != EnumCourseLevel.C1 &&
                                                                   student.PlacementResult.CurrentLevel != null)
                                                .GroupBy(student => student.PlacementResult.CurrentLevel)
                                                .OrderBy(group => group.Key)
                                                .Select(group => new DataChartModel
                                                {
                                                    Label = group.Key.ToString(),
                                                    Value = group.Count()
                                                })
                                                .ToList()
                };

                var amountStudentByLevelAndClass = filteredStudents.Where(student => student.PlacementResult.CurrentLevel != EnumCourseLevel.C1 &&
                                                                                       student.PlacementResult.CurrentLevel != null &&
                                                                                       student.PlacementResult.ProcessLevel != EnumPlacementTestLevel.IELTS &&
                                                                                       student.Student.SchoolClass != null)
                                                                     .GroupBy(student => new { student.Student.SchoolClass, student.PlacementResult.CurrentLevel })
                                                                     .Select(group => new
                                                                     {
                                                                         ClassName = group.Key.SchoolClass,
                                                                         CurrentLevel = group.Key.CurrentLevel,
                                                                         Count = group.Count()
                                                                     })
                                                                     .GroupBy(x => x.ClassName)
                                                                     .OrderBy(classGroup => classGroup.Key)
                                                                     .Select(classGroup => new StackBarChartModel
                                                                     {
                                                                         Label = classGroup.Key,
                                                                         DataColumns = classGroup.OrderBy(levelGroup => levelGroup.CurrentLevel)
                                                                                                 .Select(levelGroup => new DataChartModel
                                                                                                 {
                                                                                                     Label = levelGroup.CurrentLevel.ToString(),
                                                                                                     Value = levelGroup.Count
                                                                                                 }).ToList()
                                                                     })
                                                                     .ToList();
                if (request.ListClassName.Any())
                {
                    amountStudentByLevelAndClass = amountStudentByLevelAndClass
                                                   .Where(chart => request.ListClassName.Contains(chart.Label))
                                                   .ToList();
                }

                StackBarChartsModel stackBarCharts = new StackBarChartsModel
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

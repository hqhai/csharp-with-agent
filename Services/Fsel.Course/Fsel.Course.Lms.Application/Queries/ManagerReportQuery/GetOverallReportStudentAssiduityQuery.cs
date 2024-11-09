// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetOverallReportStudentAssiduityQuery : SearchReportAssiduityQueryModel, IRequest<MethodResult<OverallReportStudentAssiduityModel>>
    {
    }

    public class GetOverallReportStudentAssiduityQueryHandler : IRequestHandler<GetOverallReportStudentAssiduityQuery, MethodResult<OverallReportStudentAssiduityModel>>
    {
        private readonly IMediator _mediator;
        private readonly ISystemService _systemService;

        public GetOverallReportStudentAssiduityQueryHandler(IMediator mediator, ISystemService systemService)
        {
            _mediator = mediator;
            _systemService = systemService;
        }

        public async Task<MethodResult<OverallReportStudentAssiduityModel>> Handle(GetOverallReportStudentAssiduityQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OverallReportStudentAssiduityModel>();

            var userResults = await _mediator.Send(new GetStudentReportQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                SchoolClass = request.SchoolClass,
                SchoolGrade = request.SchoolGrade,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Keyword = request.Keyword,
                LearningStatus = request.LearningStatus,
                CourseType = request.CourseType,
                ManagerReportType = EnumManagerReportType.ReportStudentAssiduity
            }, cancellationToken);
            if (!userResults.IsOK)
            {
                methodResult.AddError(userResults.ErrorMessages);
                return methodResult;
            }
            var students = userResults?.Result;
            if (students == null)
            {
                methodResult.Result = new OverallReportStudentAssiduityModel();
                return methodResult;
            }
            var studentIds = students.Select(x => x.Id).ToList();
            var featureAccessTimesQuery = new FeatureAccessTimesQueryModel
            {
                EndDate = request.EndDate,
                StartDate = request.StartDate,
                FeatureAccessTimes = students.SelectMany(item =>
                {
                    var userId = item.UserId ?? default;
                    var query = new List<FeatureAccessTimeQueryModel> {
                        new FeatureAccessTimeQueryModel
                        {
                            UserId = userId,
                            CourseId = item.CourseId
                        }
                    };
                    return query;
                }).ToList()
            };
            var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeToModulesAsync(featureAccessTimesQuery);
            if (!featureAccessTimeResult.IsSuccessStatusCode)
            {
                methodResult.AddError(featureAccessTimeResult.Error);
                return methodResult;
            }
            var featureAccessTimes = featureAccessTimeResult.Content?.Result;
            var overallReport = new OverallReportStudentAssiduityModel
            {
                TotalStudent = studentIds?.Count ?? default,
                TotalAvgProgressTime = NumberHelper.ConvertRound(featureAccessTimes?.Average(x => x.AccessTime) ?? default),
                TotalAvgVisit = NumberHelper.ConvertRound(featureAccessTimes?.Average(x => x.Visit) ?? default),
            };
            methodResult.Result = overallReport;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}

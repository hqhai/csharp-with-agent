// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using System.Collections.Generic;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using static Fsel.Shared.Constants.ValueSettings;

    public class GetOverallReportLearningProgressQuery : SearchReportLearningProgressQueryModel, IRequest<MethodResult<OverallReportLearningProgressModel>>
    {
    }

    public class GetOverallReportLearningProgressQueryHandler : IRequestHandler<GetOverallReportLearningProgressQuery, MethodResult<OverallReportLearningProgressModel>>
    {
        private readonly IMediator _mediator;
        private readonly ManagerProgressHelper _managerProgressHelper;

        public GetOverallReportLearningProgressQueryHandler(IMediator mediator, ManagerProgressHelper managerProgressHelper)
        {
            _mediator = mediator;
            _managerProgressHelper = managerProgressHelper;
        }

        public async Task<MethodResult<OverallReportLearningProgressModel>> Handle(GetOverallReportLearningProgressQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OverallReportLearningProgressModel>();
            var userResults = await _mediator.Send(new GetStudentReportQuery
            {
                Keyword = request.Keyword,
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                ListCourseType = request.ListCourseType,
                ListCourseLevel = request.ListCourseLevel,

                SchoolClass = request.SchoolClass,
                SchoolGrade = request.SchoolGrade,
                EndDate = request.EndDate,
                LearningStatus = request.LearningStatus,
                CourseType = request.CourseType,
                CourseLevel = request.CourseLevel,
                ManagerReportType = EnumManagerReportType.ReportLearningProgress,
            }, cancellationToken);
            if (!userResults.IsOK)
            {
                methodResult.AddError(userResults.ErrorMessages);
                return methodResult;
            }
            var students = userResults?.Result;
            var overallReport = new OverallReportLearningProgressModel
            {
                TotalStudent = students?.Count ?? default,
                CourseLevelProgresses = EnumCourseLevelHelper.GetEnumCourseLevels(request.CourseType).Select(item => new CourseLevelProgressModel
                {
                    CourseLevel = item,
                    TotalStudent = students?.Where(x => x.CourseLevel == item).Count() ?? default
                }).ToList(),
                CourseTypeStudents = ConvertHelper.EnumToList<EnumCourseType>()
                .Select(courseType =>
                {
                    var courseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(courseType).ToHashSet();
                    return new CourseTypeStudentModel
                    {
                        CourseType = courseType,
                        TotalStudent = students?.Where(x => x.CourseLevel.HasValue && courseLevels.Contains(x.CourseLevel.Value)).Count() ?? default,
                    };
                }).ToList(),
            };

            await SetAverageProgress(overallReport, request, students);

            methodResult.Result = overallReport;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task SetAverageProgress(OverallReportLearningProgressModel overallReport, GetOverallReportLearningProgressQuery request, IList<StudentDtoModel>? students)
        {
            if (students == null || !students.Any())
            {
                overallReport.ContentAverageProgress = $"{ValueDefault} / {GetTotalProgress(request)}";
                return;
            }
            var courseResults = students.Select(x => new CourseResultModel { CourseId = x.CourseId.GetValueOrDefault(), StudentId = x.Id }).ToList();
            var countProgress = await _managerProgressHelper.GetOverallCompleteAsync(courseResults, request.EndDate);

            var totalProgress = await _managerProgressHelper.GetTotalCompleteCourseAsync(courseResults);
            overallReport.ContentAverageProgress = $"{countProgress} / {totalProgress}";
        }

        private static double GetTotalProgress(GetOverallReportLearningProgressQuery request)
        {
            return request.CourseType == EnumCourseType.Academic ? CourseProgressValue.ProgressAcademic : request.CourseType == EnumCourseType.Ielts ? CourseProgressValue.ProgressIELTS : ValueDefault;
        }
    }
}

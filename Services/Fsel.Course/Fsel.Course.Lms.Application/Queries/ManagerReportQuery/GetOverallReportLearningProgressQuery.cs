// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetOverallReportLearningProgressQuery : SearchReportLearningProgressQueryModel, IRequest<MethodResult<OverallReportLearningProgressModel>>
    {
        public GetOverallReportLearningProgressQuery()
        {
            ManagerReportType = EnumManagerReportType.ReportLearningProgress;
        }

        public GetOverallReportLearningProgressQuery(SearchReportLearningProgressQueryModel source)
        {
            ManagerReportType = EnumManagerReportType.ReportLearningProgress;
            CopyFrom(source);
        }
    }

    public class GetOverallReportLearningProgressQueryHandler
        : IRequestHandler<GetOverallReportLearningProgressQuery, MethodResult<OverallReportLearningProgressModel>>
    {
        private readonly IMediator _mediator;
        private readonly ManagerProgressHelper _managerProgressHelper;

        public GetOverallReportLearningProgressQueryHandler(
            IMediator mediator,
            ManagerProgressHelper managerProgressHelper)
        {
            _mediator = mediator;
            _managerProgressHelper = managerProgressHelper;
        }

        public async Task<MethodResult<OverallReportLearningProgressModel>> Handle(
            GetOverallReportLearningProgressQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<OverallReportLearningProgressModel>();

            var studentResult = await _mediator.Send(
                BuildStudentQuery(request),
                cancellationToken);

            if (!studentResult.IsOK)
            {
                methodResult.AddError(studentResult.ErrorMessages);
                return methodResult;
            }

            var students = studentResult.Result;

            var overallReport = BuildOverallReport(request, students);

            await SetAverageProgressAsync(
                overallReport,
                students,
                request.EndDate);

            methodResult.Result = overallReport;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        #region Private methods

        private static GetStudentReportQuery BuildStudentQuery(
            GetOverallReportLearningProgressQuery request)
        {
            return new GetStudentReportQuery
            {
                Keyword = request.Keyword,
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                IsLearning = request.IsLearning,
                ListCompletionStatus = request.ListCompletionStatus,
                ListLearningStatus = request.ListLearningStatus,
                ListOverallScore = request.ListOverallScore,
                EndDate = request.EndDate,
                ManagerReportType = EnumManagerReportType.ReportLearningProgress,
            };
        }

        private static OverallReportLearningProgressModel BuildOverallReport(
            GetOverallReportLearningProgressQuery request,
            IList<StudentDtoModel>? students)
        {
            var studentList = students ?? new List<StudentDtoModel>();

            return new OverallReportLearningProgressModel
            {
                TotalStudent = studentList.Count,

                //CourseLevelProgresses = EnumCourseLevelHelper
                //    .GetEnumCourseLevels(request.CourseType)
                //    .Select(level => new CourseLevelProgressModel
                //    {
                //        CourseLevel = level,
                //        TotalStudent = studentList.Count(x => x.CourseLevel == level)
                //    })
                //    .ToList(),

                //CourseTypeStudents = ConvertHelper
                //    .EnumToList<EnumCourseType>()
                //    .Select(courseType =>
                //    {
                //        var courseLevels = EnumCourseLevelHelper
                //            .GetEnumCourseLevels(courseType)
                //            .ToHashSet();

                //        return new CourseTypeStudentModel
                //        {
                //            CourseType = courseType,
                //            TotalStudent = studentList.Count(x =>
                //                x.CourseLevel.HasValue &&
                //                courseLevels.Contains(x.CourseLevel.Value))
                //        };
                //    })
                //    .ToList()
            };
        }

        private async Task SetAverageProgressAsync(
            OverallReportLearningProgressModel overallReport,
            IList<StudentDtoModel>? students,
            DateTime? endDate)
        {
            if (students == null || !students.Any())
            {
                //overallReport.ContentAverageProgress =
                //    $"{ValueDefault} / {GetTotalProgress(endDate)}";
                return;
            }

            var courseResults = students
                .Select(x => new CourseResultModel
                {
                    CourseId = x.CourseId.GetValueOrDefault(),
                    StudentId = x.Id
                })
                .ToList();

            var completedProgress = await _managerProgressHelper
                .GetOverallCompleteAsync(courseResults, endDate);

            var totalProgress = await _managerProgressHelper
                .GetTotalCompleteCourseAsync(courseResults);

            overallReport.ContentAverageProgress =
                $"{completedProgress} / {totalProgress}";
        }

        #endregion Private methods
    }
}

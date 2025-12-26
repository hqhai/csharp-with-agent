// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetReportLearningProgressStudentQuery : SearchReportLearningProgressQueryModel, IRequest<MethodResult<IList<LearningProgressModel>>>
    {
    }

    public class GetReportLearningProgressStudentQueryHandler : IRequestHandler<GetReportLearningProgressStudentQuery, MethodResult<IList<LearningProgressModel>>>
    {
        private readonly IMediator _mediator;
        private readonly ManagerProgressHelper _managerProgressHelper;

        public GetReportLearningProgressStudentQueryHandler(IMediator mediator,
            ManagerProgressHelper managerProgressHelper)
        {
            _mediator = mediator;
            _managerProgressHelper = managerProgressHelper;
        }

        public async Task<MethodResult<IList<LearningProgressModel>>> Handle(GetReportLearningProgressStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LearningProgressModel>>();
            var userResults = await _mediator.Send(new GetStudentReportQuery
            {
                Keyword = request.Keyword,
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                ListCourseLevel = request.ListCourseLevel,
                IsLearning = request.IsLearning,
                ListCompletionStatus = request.ListCompletionStatus,
                ListLearningStatus = request.ListLearningStatus,

                EndDate = request.EndDate,
                CourseType = request.CourseType,
                ManagerReportType = EnumManagerReportType.ReportLearningProgress,
                SortBy = request.SortBy,
            }, cancellationToken);

            if (!userResults.IsOK)
            {
                methodResult.AddError(userResults.ErrorMessages);
                return methodResult;
            }
            var students = userResults?.Result;
            if (students == null || !students.Any())
            {
                return methodResult;
            }
            var lists = students.Select(x => new CourseResultModel { CourseId = x.CourseId.GetValueOrDefault(), StudentId = x.Id }).ToList();
            var courseCompletes = await _managerProgressHelper.GetProgressCompleteLessonAsync(lists, request.EndDate);

            var courseCompleteDict = courseCompletes.ToDictionary(x => x.StudentId);
            var dateTimeUTC = DateTime.UtcNow;
            methodResult.Result = students.Select(item =>
            {
                courseCompleteDict.TryGetValue(item.Id, out var courseComplete);
                return new LearningProgressModel
                {
                    StudentId = item.Id,
                    FullName = item.FullName,
                    UserName = item.UserName,
                    PhoneNumber = item.PhoneNumber,
                    Email = item.Email,
                    SchoolGrade = item.SchoolGrade,
                    SchoolClass = item.SchoolClass,
                    CourseType = item.CourseLevel.GetEnumCourseType(),
                    CourseLevel = item.CourseLevel,
                    ContentProgress = $"{courseComplete?.TotalLessonDone} / {courseComplete?.TotalLesson}",
                    UnitName = $"{nameof(Domain.Entities.Unit)} {courseComplete?.UnitDisplayOrder}",
                    LessonName = $"{nameof(Lesson)} {courseComplete?.LessonDisplayOrder}",
                    SchoolName = item.School,
                    Status = item.ExpiredDate > dateTimeUTC ? EnumLearningStatus.InProgress : EnumLearningStatus.Expired,
                };
            }).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}

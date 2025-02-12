// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
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
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;

        public GetReportLearningProgressStudentQueryHandler(IMediator mediator,
            ManagerProgressHelper managerProgressHelper,
            ICourseRepository courseRepository,
            ICourseResultRepository courseResultRepository)
        {
            _mediator = mediator;
            _managerProgressHelper = managerProgressHelper;
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<IList<LearningProgressModel>>> Handle(GetReportLearningProgressStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LearningProgressModel>>();
            var userResults = await _mediator.Send(new GetStudentReportQuery
            {
                ListDistrict = request.ListDistrict,
                ListProvince = request.ListProvince,
                ListSchool = request.ListSchool,
                SchoolGrade = request.SchoolGrade,
                SchoolClass = request.SchoolClass,
                ListSchoolClass = request.ListSchoolClass,
                ListSchoolGrade = request.ListSchoolGrade,
                EndDate = request.EndDate,
                Keyword = request.Keyword,
                LearningStatus = request.LearningStatus,
                CourseType = request.CourseType,
                CourseLevel = request.CourseLevel,
                ManagerReportType = EnumManagerReportType.ReportLearningProgress,
                SortBy = request.SortBy,
            }, cancellationToken);

            if (!userResults.IsOK)
            {
                methodResult.AddError(userResults.ErrorMessages);
                return methodResult;
            }
            var students = userResults?.Result;
            if (students == null)
            {
                return methodResult;
            }
            var lists = students.Select(x => new CourseResultModel { CourseId = x.CourseId.GetValueOrDefault(), StudentId = x.Id }).ToList();
            var courseCompletes = await _managerProgressHelper.GetProgressCompleteModuleAsync(lists, request.EndDate);
            methodResult.Result = students.Select(item =>
            {
                var courseComplete = courseCompletes.FirstOrDefault(x => x.StudentId == item.Id);
                return new LearningProgressModel
                {
                    Email = item.Email,
                    FullName = item.FullName,
                    SchoolClass = item.SchoolClass,
                    SchoolGrade = item.SchoolGrade,
                    SchoolName = item.School,
                    Status = item.ExpiredDate > DateTime.UtcNow ? EnumLearningStatus.InProgress : EnumLearningStatus.Expired,
                    CourseLevel = item.CourseLevel,
                    ContentProgress = $"{courseComplete?.CountComplete} / {courseComplete?.TotalComplete}",
                    UnitName = $"{nameof(Domain.Entities.Unit)} {courseComplete?.UnitDisplayOrder}",
                    LessonName = $"{nameof(Lesson)} {courseComplete?.LessonDisplayOrder}"
                };
            }).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}

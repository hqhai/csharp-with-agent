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
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetReportLearningProgressStudentQuery : SearchReportLearningProgressQueryModel, IRequest<MethodResult<IList<LearningProgressModel>>>
    {
        public GetReportLearningProgressStudentQuery()
        {
            ManagerReportType = EnumManagerReportType.ReportLearningProgress;
        }

        public GetReportLearningProgressStudentQuery(SearchReportLearningProgressQueryModel source)
        {
            ManagerReportType = EnumManagerReportType.ReportLearningProgress;
            CopyFrom(source);
        }
    }

    public class GetReportLearningProgressStudentQueryHandler
        : IRequestHandler<GetReportLearningProgressStudentQuery, MethodResult<IList<LearningProgressModel>>>
    {
        private readonly IMediator _mediator;
        private readonly ManagerProgressHelper _managerProgressHelper;

        public GetReportLearningProgressStudentQueryHandler(
            IMediator mediator,
            ManagerProgressHelper managerProgressHelper)
        {
            _mediator = mediator;
            _managerProgressHelper = managerProgressHelper;
        }

        public async Task<MethodResult<IList<LearningProgressModel>>> Handle(GetReportLearningProgressStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<IList<LearningProgressModel>>();
            var studentResult = await _mediator.Send(new GetStudentReportQuery(request), cancellationToken);

            if (!studentResult.IsOK)
            {
                methodResult.AddError(studentResult.ErrorMessages);
                return methodResult;
            }

            var students = studentResult.Result;
            if (students == null || !students.Any())
            {
                methodResult.Result = new List<LearningProgressModel>();
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var courseRequests = students
                .Select(x => new CourseResultModel
                {
                    CourseId = x.CourseId.GetValueOrDefault(),
                    StudentId = x.Id
                })
                .ToList();

            var courseCompletes = await _managerProgressHelper.GetProgressCompleteLessonAsync(courseRequests, request.EndDate);

            var courseCompleteLookup = courseCompletes.ToDictionary(x => x.StudentId);

            var nowUtc = DateTime.UtcNow;
            methodResult.Result = students.Select(student => MapLearningProgress(
            student,
            courseCompleteLookup,
            nowUtc)).ToList();

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        #region Private methods

        private static LearningProgressModel MapLearningProgress(
            StudentDtoModel student,
            IDictionary<Guid, CourseCompleteModel> courseCompleteLookup,
            DateTime nowUtc)
        {
            courseCompleteLookup.TryGetValue(student.Id, out var courseComplete);

            return new LearningProgressModel
            {
                StudentId = student.Id,
                FullName = student.FullName,
                UserName = student.UserName,
                PhoneNumber = student.PhoneNumber,
                Email = student.Email,
                SchoolGrade = student.SchoolGrade,
                SchoolClass = student.SchoolClass,
                SchoolName = student.School,
                CourseLevel = student.CourseLevel,
                CourseType = student.CourseLevel.GetEnumCourseType(),
                ContentProgress = $"{courseComplete?.TotalLessonDone} / {courseComplete?.TotalLesson}",
                UnitName = $"{nameof(Domain.Entities.Unit)} {courseComplete?.UnitDisplayOrder}",
                LessonName = $"{nameof(Lesson)} {courseComplete?.LessonDisplayOrder}",
                Status = student.ExpiredDate > nowUtc
                    ? EnumLearningStatus.InProgress
                    : EnumLearningStatus.Expired
            };
        }

        #endregion Private methods
    }
}

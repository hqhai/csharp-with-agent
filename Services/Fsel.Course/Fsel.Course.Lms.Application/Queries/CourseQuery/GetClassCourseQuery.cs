// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetClassCourseQuery : IRequest<MethodResult<string>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetClassCourseQueryHandler : IRequestHandler<GetClassCourseQuery, MethodResult<string>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ITrainingService _trainingService;
        private readonly ICourseClassStudentRepository _courseClassStudentRepository;

        public GetClassCourseQueryHandler(
            ICourseRepository courseRepository,
            ITrainingService trainingService,
            ICourseClassStudentRepository courseClassStudentRepository)
        {
            _courseRepository = courseRepository;
            _trainingService = trainingService;
            _courseClassStudentRepository = courseClassStudentRepository;
        }

        public async Task<MethodResult<string>> Handle(GetClassCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<string> methodResult = new MethodResult<string>();

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseIdNotExist), nameof(request.CourseId), request.CourseId);
                return methodResult;
            }

            var classContents = await _trainingService.GetClassByStatusNewAsync();
            var classNews = classContents?.Content?.Result;
            var classIds = classNews?.Select(x => x.Id).ToList();
            var courseClassStudent = await _courseClassStudentRepository.GetIncludeByIdsAsync(classIds, course.Id);

            if (classNews == null || classNews.Count == 0 || courseClassStudent == null)
            {
                var code = await _trainingService.GetNewClassCodeAsync(course.CourseLevel);
                var classcode = code?.Content?.Result;
                if (classcode == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.ClasseCodeNotExist), nameof(request.CourseId), request.CourseId);
                    return methodResult;
                }
                methodResult.Result = classcode;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var classes = classNews.FirstOrDefault(e => e.Id == courseClassStudent.ClassId);
            if (classes == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.ClasseIdNotExist), nameof(courseClassStudent.ClassId), courseClassStudent.ClassId);
                return methodResult;
            }
            methodResult.Result = classes.Code;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}

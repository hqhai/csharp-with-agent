// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

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
                methodResult.AddErrorBadRequest(
               nameof(EnumCourseErrorCode.CourseNotExist),
               nameof(request.CourseId), request.CourseId);
                return methodResult;
            }
            var classContents = await _trainingService.GetClassByStatusNewAsync();

            var classnews = classContents?.Content?.Result;

            if (classnews != null && classnews.Count > 0)
            {
                var courseClassStudents = await _courseClassStudentRepository.Queryable.Where(e => e.CourseId == request.CourseId)
                                                            .Where(e => classnews.Select(x => x.Id).Contains(e.ClassId))
                                                            .ToListAsync(cancellationToken: cancellationToken);
                if (courseClassStudents != null && courseClassStudents.Count > 0)
                {
                    var classes = classnews.Where(e => courseClassStudents.Select(x => x.ClassId).Contains(e.Id)).ToList();
                    if (classes == null || classes.FirstOrDefault() == null)
                    {
                        methodResult.AddErrorBadRequest(
                      nameof(EnumCourseErrorCode.ClassesNotExitst),
                      nameof(request.CourseId), request.CourseId);
                        return methodResult;
                    }
                    else
                    {
                        var classs = classes.FirstOrDefault();
                        methodResult.Result = classs?.Code;
                        methodResult.StatusCode = StatusCodes.Status200OK;
                        return methodResult;
                    }
                }
            }
            else
            {
                var code = await _trainingService.GetNewClassCodeAsync(course.CourseLevel);
                var classcode = code?.Content?.Result;
                if (classcode == null)
                {
                    methodResult.AddErrorBadRequest(
                   nameof(EnumCourseErrorCode.CourseNotInClass),
                   nameof(request.CourseId), request.CourseId);
                    return methodResult;
                }
                methodResult.Result = classcode;
                methodResult.StatusCode = StatusCodes.Status200OK;
            }
            return methodResult;
        }
    }
}

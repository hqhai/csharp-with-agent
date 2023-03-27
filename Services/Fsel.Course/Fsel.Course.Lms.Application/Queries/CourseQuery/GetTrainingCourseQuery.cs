// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.TrainingServices.Models;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTrainingCourseQuery : IRequest<MethodResult<ClassModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetClassCourseQueryHandler : IRequestHandler<GetTrainingCourseQuery, MethodResult<ClassModel>>
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

        public async Task<MethodResult<ClassModel>> Handle(GetTrainingCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(
               nameof(EnumCourseErrorCode.CourseNotExist),
               nameof(request.CourseId), request.CourseId);
                return methodResult;
            }
            var trainingContents = await _trainingService.GetTrainingByStatusNewAsync();

            var classnews = trainingContents?.Content?.Result;
            if (classnews != null && classnews.Count > 0)
            {
                var courseClassStudents = await _courseClassStudentRepository.Queryable.Where(e => e.CourseId == request.CourseId)
                                                            .Where(e => classnews.Select(x => x.Id).Contains(e.ClassId))
                                                            .ToListAsync(cancellationToken: cancellationToken);
                if (courseClassStudents != null && courseClassStudents.Count > 0)
                {
                    var classes = classnews.Where(e => courseClassStudents.Select(x => x.ClassId).Contains(e.Id)).ToList();
                    if (classes == null || classes.Count == 0)
                    {
                        methodResult.AddErrorBadRequest(
                      nameof(EnumCourseErrorCode.ClassesNotExitst),
                      nameof(request.CourseId), request.CourseId);
                        return methodResult;
                    }
                    else
                    {
                        methodResult.Result = classes.FirstOrDefault();
                        methodResult.StatusCode = StatusCodes.Status200OK;
                        return methodResult;
                    }
                }
                else
                {
                    var code = await _trainingService.GetNewTrainingCodeAsync(course.CourseLevel);
                    var trainingcode = code?.Content?.Result;
                    if (trainingcode == null)
                    {
                        methodResult.AddErrorBadRequest(
                       nameof(EnumCourseErrorCode.CourseNotInClass),
                       nameof(request.CourseId), request.CourseId);
                        return methodResult;
                    }
                    methodResult.Result = new ClassModel { Code = trainingcode };
                    methodResult.StatusCode = StatusCodes.Status200OK;
                    return methodResult;
                }
            }
            methodResult.AddErrorBadRequest(
                     nameof(EnumCourseErrorCode.ClassesNewNotExitst));
            methodResult.StatusCode = StatusCodes.Status400BadRequest;
            return methodResult;
        }
    }
}

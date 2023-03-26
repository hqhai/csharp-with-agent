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

    public class GetTrainingCourseQuery : IRequest<MethodResult<TrainingModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetClassCourseQueryHandler : IRequestHandler<GetTrainingCourseQuery, MethodResult<TrainingModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ITrainingService _trainingService;
        private readonly ICourseTrainingRepository _courseTrainingRepository;

        public GetClassCourseQueryHandler(
            ICourseRepository courseRepository,
            ITrainingService trainingService,
            ICourseTrainingRepository courseTrainingRepository)
        {
            _courseRepository = courseRepository;
            _trainingService = trainingService;
            _courseTrainingRepository = courseTrainingRepository;
        }

        public async Task<MethodResult<TrainingModel>> Handle(GetTrainingCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TrainingModel> methodResult = new MethodResult<TrainingModel>();

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(
               nameof(EnumCourseErrorCode.CourseNotExist),
               nameof(request.CourseId), request.CourseId);
                return methodResult;
            }
            var trainingContents = await _trainingService.GetTrainingByStatusNewAsync();

            var trainings = trainingContents?.Content?.Result;
            if (trainings != null && trainings.Count > 0)
            {
                var CourseClasses = await _courseTrainingRepository.Queryable.Where(e => e.CourseId == request.CourseId)
                                                            .Where(e => trainings.Select(x => x.Id).Contains(e.ClassId))
                                                            .ToListAsync(cancellationToken: cancellationToken);
                if (CourseClasses != null && CourseClasses.Count > 0)
                {
                    var training = trainings.Where(e => CourseClasses.Select(e => e.ClassId).Contains(e.Id)).FirstOrDefault();
                    methodResult.Result = training;
                    methodResult.StatusCode = StatusCodes.Status200OK;
                    return methodResult;
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
                    methodResult.Result = new TrainingModel { Code = trainingcode };
                    methodResult.StatusCode = StatusCodes.Status200OK;
                }
            }
            methodResult.AddErrorBadRequest(
                     nameof(EnumCourseErrorCode.ClassesNewNotExitst));
            methodResult.StatusCode = StatusCodes.Status400BadRequest;
            return methodResult;
        }
    }
}

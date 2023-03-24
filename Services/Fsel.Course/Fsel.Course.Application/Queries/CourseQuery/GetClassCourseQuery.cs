// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.CourseQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Application.Services;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassCourseQuery : IRequest<MethodResult<ClassModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetClassCourseQueryHandler : IRequestHandler<GetClassCourseQuery, MethodResult<ClassModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IClassService _classService;
        private readonly ICourseClassRepository _courseClassRepository;

        public GetClassCourseQueryHandler(
            ICourseRepository courseRepository,
            IClassService classService,
            ICourseClassRepository courseClassRepository)
        {
            _courseRepository = courseRepository;
            _classService = classService;
            _courseClassRepository = courseClassRepository;
        }

        public async Task<MethodResult<ClassModel>> Handle(GetClassCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            {
                methodResult.AddErrorBadRequest(
               nameof(EnumCourseErrorCode.CourseNotExist),
               nameof(request.CourseId), request.CourseId);
                return methodResult;
            }

            var classnews = await _classService.GetClassByStatusNewAsync();
            var courseClasses = await _courseClassRepository.Queryable.Where(e => e.CourseId == request.CourseId)
                                                              .Where(e => classnews.Content.Result.Any(x => x.Id == e.ClassId))
                                                              .ToListAsync(cancellationToken: cancellationToken);
            if (courseClasses == null)
            {
                var code = await _classService.GetNewClassCodeAsync(course.CourseLevel);
                if (code == null)
                {
                    methodResult.AddErrorBadRequest(
                   nameof(EnumCourseErrorCode.CourseNotInClass),
                   nameof(request.CourseId), request.CourseId);
                    return methodResult;
                }
                methodResult.Result = new ClassModel { Code = code.Content.Result };
                methodResult.StatusCode = StatusCodes.Status200OK;
            }
            var classes = classnews.Content.Result.Where(e => courseClasses.Select(e => e.ClassId).Contains(e.Id)).FirstOrDefault();
            methodResult.Result = classes;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}

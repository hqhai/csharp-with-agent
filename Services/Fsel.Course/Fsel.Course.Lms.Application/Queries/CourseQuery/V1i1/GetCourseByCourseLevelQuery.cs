// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery.V1i1
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseByCourseLevelQuery : IRequest<MethodResult<CourseModel>>
    {
        public EnumCourseLevel CourseLevel { get; set; }
    }

    public class GetCourseByCourseLevelQueryHandler : IRequestHandler<GetCourseByCourseLevelQuery, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;

        public GetCourseByCourseLevelQueryHandler(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<CourseModel>> Handle(GetCourseByCourseLevelQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CourseModel>();
            var course = await _courseRepository.Queryable
                                .Where(x => x.CourseLevel == request.CourseLevel && x.Status != EnumCourseStatus.New && x.Status != EnumCourseStatus.Clone)
                                .OrderByDescending(x => x.CreatedDate)
                                .ThenByDescending(x => x.UpdatedDate)
                                .Select(x => new CourseModel
                                {
                                    Id = x.Id,
                                    Name = x.Name,
                                    CourseLevel = x.CourseLevel,
                                    CreatedDate = x.CreatedDate,
                                }).FirstOrDefaultAsync(cancellationToken);

            methodResult.Result = course;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}

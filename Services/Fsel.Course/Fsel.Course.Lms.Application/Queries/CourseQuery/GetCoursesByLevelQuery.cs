// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCoursesByLevelQuery : IRequest<MethodResult<IList<CourseModel>>>
    {
        public EnumCourseLevel? CourseLevel { get; set; }
    }

    public class GetCourseByCourseLevelQueryHandler : IRequestHandler<GetCoursesByLevelQuery, MethodResult<IList<CourseModel>>>
    {
        private readonly ICourseRepository _courseRepository;

        public GetCourseByCourseLevelQueryHandler(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<IList<CourseModel>>> Handle(GetCoursesByLevelQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CourseModel>>();
            var courses = await _courseRepository.Queryable
                                .Where(x => request.CourseLevel != null && x.CourseLevel == request.CourseLevel)
                                .Select(x => new CourseModel
                                {
                                    Id = x.Id,
                                    CourseLevel = x.CourseLevel,
                                    CreatedDate = x.CreatedDate,
                                }).ToListAsync(cancellationToken);

            methodResult.Result = courses;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}

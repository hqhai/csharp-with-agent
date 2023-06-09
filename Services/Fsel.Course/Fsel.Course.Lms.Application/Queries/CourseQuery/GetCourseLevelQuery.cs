// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseLevelQuery : IRequest<MethodResult<CourseModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetCourseLevelQueryHandler : IRequestHandler<GetCourseLevelQuery, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;

        public GetCourseLevelQueryHandler(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<CourseModel>> Handle(GetCourseLevelQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<CourseModel>();

            var course = await _courseRepository.Queryable.Select(x => new CourseModel
            {
                Id = x.Id,
                CourseLevel = x.CourseLevel,
                CreatedDate = x.CreatedDate,
            }).FirstOrDefaultAsync(cancellationToken);

            methodResult.Result = course;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}

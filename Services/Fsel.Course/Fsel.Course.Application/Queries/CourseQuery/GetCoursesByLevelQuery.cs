// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.CourseQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCoursesByLevelQuery : IRequest<MethodResult<List<CourseModel>>>
    {
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseStatus? Status { get; set; }
    }

    public class GetCoursesByLevelQueryHandler : IRequestHandler<GetCoursesByLevelQuery, MethodResult<List<CourseModel>>>
    {
        private readonly ICourseRepository _courseRepository;

        public GetCoursesByLevelQueryHandler(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<List<CourseModel>>> Handle(GetCoursesByLevelQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<List<CourseModel>> methodResult = new MethodResult<List<CourseModel>>();

            var courses = await _courseRepository.Queryable.Where(p => !p.IsArchive && p.CourseLevel == request.CourseLevel && (!request.Status.HasValue || request.Status == p.Status)).Select(x => new CourseModel
            {
                Id = x.Id,
                CourseLevel = x.CourseLevel,
                Name = x.Name,
                Code = x.Code,
                CourseType = x.CourseType,
                Status = x.Status,
            }).ToListAsync(cancellationToken);
            methodResult.Result = courses;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}

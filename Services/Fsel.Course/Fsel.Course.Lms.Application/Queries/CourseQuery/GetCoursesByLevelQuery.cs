// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCoursesByLevelQuery : IRequest<MethodResult<IList<CourseModel>>>
    {
        public EnumCourseStatus? DifferentStatus { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
    }

    public class GetCoursesByLevelQueryHandler : IRequestHandler<GetCoursesByLevelQuery, MethodResult<IList<CourseModel>>>
    {
        private readonly ICourseRepository _courseRepository;

        public GetCoursesByLevelQueryHandler(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<IList<CourseModel>>> Handle(GetCoursesByLevelQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CourseModel>>();
            var courses = await _courseRepository.Queryable.Where(x => !x.IsArchive)
                                .Where(x => !request.DifferentStatus.HasValue || x.Status != request.DifferentStatus)
                                .Where(x => request.CourseLevel != null && x.CourseLevel == request.CourseLevel)
                                .Select(x => new CourseModel
                                {
                                    Id = x.Id,
                                    Code = x.Code,
                                    Status = x.Status,
                                    Name = x.Name,
                                    CourseLevel = x.CourseLevel,
                                    CreatedDate = x.CreatedDate,
                                    UpdatedDate = x.UpdatedDate,
                                }).OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                                .ToListAsync(cancellationToken);

            methodResult.Result = courses;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}

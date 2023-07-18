// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Querys.CourseTimeConfigQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseTimeConfigByListCourseIdQuery : IRequest<MethodResult<IList<CourseTimeConfigModel>>>
    {
        public IList<Guid>? CourseIds { get; set; }
    }

    public class GetCourseTimeConfigByCourseIdQueryHandler : IRequestHandler<GetCourseTimeConfigByListCourseIdQuery, MethodResult<IList<CourseTimeConfigModel>>>
    {
        private readonly ICourseTimeConfigRepository _courseTimeConfigRepository;

        public GetCourseTimeConfigByCourseIdQueryHandler(ICourseTimeConfigRepository courseTimeConfigRepository)
        {
            _courseTimeConfigRepository = courseTimeConfigRepository;
        }

        public async Task<MethodResult<IList<CourseTimeConfigModel>>> Handle(GetCourseTimeConfigByListCourseIdQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<IList<CourseTimeConfigModel>>();

            var courseTimeConfig = await _courseTimeConfigRepository.Queryable
                                        .Where(x => request.CourseIds!.Contains(x.CourseId))
                                        .Select(x => new CourseTimeConfigModel
                                        {
                                            CourseId = x.CourseId,
                                            Id = x.Id,
                                            CreatedDate = x.CreatedDate,
                                            DurationMonth = x.DurationMonth,
                                            EnrollmentWeek = x.EnrollmentWeek,
                                        }).ToListAsync(cancellationToken);

            methodResult.Result = courseTimeConfig;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}

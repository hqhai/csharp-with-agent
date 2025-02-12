// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseToIntegrationQuery : IRequest<MethodResult<IList<object>>>
    {
        public required IList<Guid> UserIds { get; set; }
    }

    public class GetCourseToIntegrationQueryHandler : IRequestHandler<GetCourseToIntegrationQuery, MethodResult<IList<object>>>
    {
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IMapper _mapper;

        public GetCourseToIntegrationQueryHandler(ICourseResultRepository courseResultRepository, IMapper mapper)
        {
            _courseResultRepository = courseResultRepository;
            _mapper = mapper;
        }
        public async Task<MethodResult<IList<object>>> Handle(GetCourseToIntegrationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<object>> methodResult = new MethodResult<IList<object>>();

            var query = await _courseResultRepository.Queryable
                                                     .Include(x => x.Course)
                                                     .WhereBulkContains(request.UserIds, x => x.CreatedUserId)
                                                     .Select(x => new
                                                     {
                                                         UserId = x.CreatedUserId,
                                                         CourseId = x.CourseId,
                                                         CourseLevel = x.Course != null ? x.Course.CourseLevel.ToString() : string.Empty,
                                                         Status = x.Status.ToString(),
                                                     })
                                                     .ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<IList<object>>(query);
            return methodResult;
        }
    }
}

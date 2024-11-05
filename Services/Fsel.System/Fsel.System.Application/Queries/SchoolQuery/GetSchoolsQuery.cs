// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.SchoolQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using global::System.Collections.Generic;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSchoolsQuery : IRequest<MethodResult<IList<Guid>>>
    {
        public IList<Guid> SchoolIds { get; set; } = new List<Guid>();
        public IList<Guid> ProvinceIds { get; set; } = new List<Guid>();
        public IList<Guid> DistrictIds { get; set; } = new List<Guid>();
    }

    public class GetSchoolsQueryHandler : IRequestHandler<GetSchoolsQuery, MethodResult<IList<Guid>>>
    {
        private readonly ICrmLocationRepository _locationCrmRepository;

        public GetSchoolsQueryHandler(ICrmLocationRepository locationCrmRepository)
        {
            _locationCrmRepository = locationCrmRepository;
        }

        public async Task<MethodResult<IList<Guid>>> Handle(GetSchoolsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<Guid>> methodResult = new MethodResult<IList<Guid>>();

            var districtIds = await GetDistrictIdsAsync(request);
            var query = _locationCrmRepository.Queryable;
            if (request.SchoolIds != null && request.SchoolIds.Any())
            {
                query = query.Where(x => request.SchoolIds.Any(y => y == x.GlobalId));
            }
            if (districtIds.Any())
            {
                query = query.Where(x => x.ParentId.HasValue && districtIds.Any(y => y == x.ParentId.Value));
            }
            methodResult.Result = await query.Select(x => x.GlobalId).Distinct().ToListAsync(cancellationToken);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<IList<int>> GetDistrictIdsAsync(GetSchoolsQuery request)
        {
            var query = _locationCrmRepository.Queryable;
            if (request.ProvinceIds.Any())
            {
                var parentIds = await _locationCrmRepository.Queryable.Where(x => request.ProvinceIds.Any(y => y == x.GlobalId)).Select(x => x.Id).ToListAsync();
                query = query.Where(x => x.ParentId.HasValue && parentIds.Any(y => y == x.ParentId.Value));
            }
            if (request.DistrictIds.Any())
            {
                query = query.Where(x => request.DistrictIds.Any(y => y == x.GlobalId));
            }
            return await query.Select(x => x.Id).ToListAsync();
        }
    }
}

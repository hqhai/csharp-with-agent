// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.LocationQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetLocationsByIdsQuery : GetLocationsByIdsQueryModel, IRequest<MethodResult<IList<LocationModel>>>
    {
    }

    public class GetLocationByIdsQueryHandler : IRequestHandler<GetLocationsByIdsQuery, MethodResult<IList<LocationModel>>>
    {
        private readonly ILocationRepository _locationRepository;
        private readonly IMapper _mapper;

        public GetLocationByIdsQueryHandler(ILocationRepository locationRepository, IMapper mapper)
        {
            _locationRepository = locationRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<LocationModel>>> Handle(GetLocationsByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LocationModel>>();

            if (request.Ids == null || request.Ids.Count == 0)
            {
                return methodResult;
            }

            var locations = await _locationRepository.Queryable.Where(p => request.Ids.Contains(p.Id)).ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map<IList<LocationModel>>(locations);
            return methodResult;
        }
    }
}

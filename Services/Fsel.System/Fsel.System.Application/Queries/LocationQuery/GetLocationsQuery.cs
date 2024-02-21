// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.LocationQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetLocationsQuery : IRequest<MethodResult<IList<LocationModel>>>
    {
        public EnumLocationType LocationType { get; set; }
        public Guid? ParentId { get; set; }
    }

    public class GetLocationsQueryHandler : IRequestHandler<GetLocationsQuery, MethodResult<IList<LocationModel>>>
    {
        private readonly ILocationRepository _locationRepository;

        public GetLocationsQueryHandler(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        public async Task<MethodResult<IList<LocationModel>>> Handle(GetLocationsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LocationModel>>();

            var locations = await _locationRepository.Queryable.Where(p => p.Type == request.LocationType).ToListAsync(cancellationToken);
            if (request.ParentId.HasValue)
            {
                locations = locations.Where(p => p.ParentId == request.ParentId).ToList();
            }
            methodResult.Result = locations.Select(p => new LocationModel
            {
                Id = p.Id,
                Code = p.UrBoxId,
                Name = p.Name,
            }).ToList();
            return methodResult;
        }
    }
}

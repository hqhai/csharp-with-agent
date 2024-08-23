// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.LocationQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.QueryModels;
    using global::System.Linq;
    using MediatR;

    public class GetDetailLocationQuery : GetLocationsDetailQueryModel, IRequest<MethodResult<IList<LocationModel>>>
    {
    }

    public class GetDetailLocationQueryHandler : IRequestHandler<GetDetailLocationQuery, MethodResult<IList<LocationModel>>>
    {
        private readonly ILocationRepository _locationRepository;
        private readonly IMapper _mapper;

        public GetDetailLocationQueryHandler(ILocationRepository locationRepository, IMapper mapper)
        {
            _locationRepository = locationRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<LocationModel>>> Handle(GetDetailLocationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LocationModel>>();

            var locationInfo = _locationRepository.Queryable
                                        .Where(x => x.Id == request.CountryId ||
                                                    (x.Id == request.CityId && x.ParentId == request.CountryId) ||
                                                    (x.Id == request.DistrictId && x.ParentId == request.CityId))
                                        .ToList();

            methodResult.Result = _mapper.Map<IList<LocationModel>>(locationInfo);
            return methodResult;
        }
    }
}

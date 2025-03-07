// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.LocationQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Entities;
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
        private readonly IMapper _mapper;
        private readonly ICrmLocationRepository _locationCrmRepository;

        public GetDetailLocationQueryHandler(IMapper mapper, ICrmLocationRepository locationCrmRepository)
        {
            _mapper = mapper;
            _locationCrmRepository = locationCrmRepository;
        }

        public async Task<MethodResult<IList<LocationModel>>> Handle(GetDetailLocationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LocationModel>>();

            var query = _locationCrmRepository.Queryable.Where(p => p.TypeName == EnumCrmLocationTypeName.Site);

            var country = query.Where(x => x.GlobalId == request.CountryId).FirstOrDefault();
            var zones = query.Where(x => country != null && x.ParentId == country.Id).ToList();
            var city = query.Where(x => x.GlobalId == request.CityId && zones != null && zones.Select(n => n.Id).Contains(x.ParentId ?? default)).FirstOrDefault();
            var district = query.Where(x => x.GlobalId == request.DistrictId && city != null && x.ParentId == city.Id).FirstOrDefault();

            methodResult.Result = _mapper.Map<IList<LocationModel>>(district ?? city ?? country);
            return methodResult;
        }
    }
}

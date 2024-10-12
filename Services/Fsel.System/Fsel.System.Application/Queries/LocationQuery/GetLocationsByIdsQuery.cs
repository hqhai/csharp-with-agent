// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.LocationQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLocationsByIdsQuery : GetLocationsByIdsQueryModel, IRequest<MethodResult<IList<LocationModel>>>
    {
    }

    public class GetLocationByIdsQueryHandler : IRequestHandler<GetLocationsByIdsQuery, MethodResult<IList<LocationModel>>>
    {
        private readonly IMapper _mapper;
        private readonly ICrmLocationRepository _locationCrmRepository;

        public GetLocationByIdsQueryHandler(ICrmLocationRepository locationCrmRepository, IMapper mapper)
        {
            _locationCrmRepository = locationCrmRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<LocationModel>>> Handle(GetLocationsByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LocationModel>>();

            if (request.Ids == null || !request.Ids.Any())
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var locations = await _locationCrmRepository.Queryable.Where(p => request.Ids.Contains(p.GlobalId) && p.TypeName == EnumCrmLocationTypeName.Site).ToListAsync(cancellationToken);
            if (locations == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(locations));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<IList<LocationModel>>(locations);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}

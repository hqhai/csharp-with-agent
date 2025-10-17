// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.LocationQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLocationByGlobalIdQuery : IRequest<MethodResult<LocationModel>>
    {
        public string? GlobalId { get; set; }
    }

    public class GetLocationByGlobalIdQueryHandler : IRequestHandler<GetLocationByGlobalIdQuery, MethodResult<LocationModel>>
    {
        private readonly ICrmLocationRepository _locationRepository;
        private readonly IMapper _mapper;

        public GetLocationByGlobalIdQueryHandler(ICrmLocationRepository locationRepository, IMapper mapper)
        {
            _locationRepository = locationRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<LocationModel>> Handle(GetLocationByGlobalIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.GlobalId);
            MethodResult<LocationModel> methodResult = new MethodResult<LocationModel>();

            var location = await _locationRepository.Queryable.FirstOrDefaultAsync(x => x.GlobalId.ToString() == request.GlobalId, cancellationToken);

            if (location == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(location));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<LocationModel>(location);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}

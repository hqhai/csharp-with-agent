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

    public class GetLocationByLocalIdQuery : IRequest<MethodResult<LocationModel>>
    {
        public string? LocalId { get; set; }
    }

    public class GetLocationByLocalIdQueryHandler : IRequestHandler<GetLocationByLocalIdQuery, MethodResult<LocationModel>>
    {
        private readonly ICrmLocationRepository _crmLocationRepository;
        private readonly IMapper _mapper;

        public GetLocationByLocalIdQueryHandler(ICrmLocationRepository crmLocationRepository,
                                                IMapper mapper)
        {
            _crmLocationRepository = crmLocationRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<LocationModel>> Handle(GetLocationByLocalIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.LocalId);
            MethodResult<LocationModel> methodResult = new MethodResult<LocationModel>();

            var location = await _crmLocationRepository.Queryable
                                                       .Include(x => x.Parent)
                                                       .FirstOrDefaultAsync(x => x.LocalId != null && x.LocalId.Trim() == request.LocalId.Trim(), cancellationToken);

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

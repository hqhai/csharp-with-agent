// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.BlindBoxes
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories.BlindBoxes;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetBlindBoxQuery : IRequest<MethodResult<BlindBoxModel>>
    {
    }

    public class GetBlindBoxQueryHandler : IRequestHandler<GetBlindBoxQuery, MethodResult<BlindBoxModel>>
    {
        private readonly IBlindBoxRepository _blindBoxRepository;
        private readonly IMapper _mapper;

        public GetBlindBoxQueryHandler(IBlindBoxRepository blindBoxRepository, IMapper mapper)
        {
            _blindBoxRepository = blindBoxRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<BlindBoxModel>> Handle(GetBlindBoxQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<BlindBoxModel>();

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var blindBox = await _blindBoxRepository.Queryable.Include(p => p.BlindBoxChests).FirstOrDefaultAsync(p => p.StartDate <= currentDate && p.EndDate >= currentDate, cancellationToken);

            if (blindBox == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumBuyBlindBoxErrorCode.EventHasExpired), nameof(blindBox));
                return methodResult;
            }

            var blindBoxModel = _mapper.Map<BlindBoxModel>(blindBox);

            methodResult.Result = blindBoxModel;
            return methodResult;
        }
    }
}
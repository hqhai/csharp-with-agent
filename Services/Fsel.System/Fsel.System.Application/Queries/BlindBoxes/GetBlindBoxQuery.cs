// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.BlindBoxes
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
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
            var blindBox = await _blindBoxRepository.Queryable.FirstOrDefaultAsync(cancellationToken);
            methodResult.Result = _mapper.Map<BlindBoxModel>(blindBox);
            return methodResult;
        }
    }
}

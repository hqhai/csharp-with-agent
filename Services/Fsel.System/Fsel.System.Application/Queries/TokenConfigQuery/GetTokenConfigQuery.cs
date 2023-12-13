// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.TokenConfigQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTokenConfigQuery : IRequest<MethodResult<TokenConfigModel>>
    {
        public EnumTokenFeature Feature { get; set; }
        public EnumTokenMission Mission { get; set; }
    }

    public class GetTokenConfigQueryHandler : IRequestHandler<GetTokenConfigQuery, MethodResult<TokenConfigModel>>
    {
        private readonly ITokenConfigRepository _tokenConfigRepository;
        private readonly IMapper _mapper;

        public GetTokenConfigQueryHandler(ITokenConfigRepository tokenConfigRepository, IMapper mapper)
        {
            _tokenConfigRepository = tokenConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<TokenConfigModel>> Handle(GetTokenConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<TokenConfigModel>();
            var tokenConfig = await _tokenConfigRepository.Queryable.FirstOrDefaultAsync(x => x.Feature == request.Feature && x.Mission == request.Mission, cancellationToken);
            if (tokenConfig == null)
            {
                return methodResult;
            }
            methodResult.Result = _mapper.Map<TokenConfigModel>(tokenConfig);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}

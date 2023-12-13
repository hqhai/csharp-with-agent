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

    public class GetTokenConfigsQuery : IRequest<MethodResult<IList<TokenConfigModel>>>
    {
        public EnumTokenFeature Feature { get; set; }
        public IList<EnumTokenMission>? Missions { get; set; }
    }

    public class GetTokenConfigsQueryHandler : IRequestHandler<GetTokenConfigsQuery, MethodResult<IList<TokenConfigModel>>>
    {
        private readonly ITokenConfigRepository _tokenConfigRepository;
        private readonly IMapper _mapper;

        public GetTokenConfigsQueryHandler(ITokenConfigRepository tokenConfigRepository, IMapper mapper)
        {
            _tokenConfigRepository = tokenConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<TokenConfigModel>>> Handle(GetTokenConfigsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<TokenConfigModel>>();
            var tokenConfigs = await _tokenConfigRepository.Queryable.Where(x => x.Feature == request.Feature && request.Missions != null && request.Missions.Contains(x.Mission)).ToListAsync(cancellationToken);
            if (tokenConfigs == null || !tokenConfigs.Any())
            {
                return methodResult;
            }
            methodResult.Result = _mapper.Map<IList<TokenConfigModel>>(tokenConfigs);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}

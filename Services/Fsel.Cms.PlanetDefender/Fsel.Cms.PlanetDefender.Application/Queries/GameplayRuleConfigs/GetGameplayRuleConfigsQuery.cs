// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.GameplayRuleConfigs
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.GameplayRuleConfigs;
    using Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.GameplayTimeConfigs;
    using Fsel.Common.ActionResults;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetGameplayRuleConfigsQuery : IRequest<MethodResult<IList<GameplayRuleConfigsModel>>>
    {
    }

    public class GetGameplayRuleConfigsQueryHandler : IRequestHandler<GetGameplayRuleConfigsQuery, MethodResult<IList<GameplayRuleConfigsModel>>>
    {
        private readonly IGameplayRuleConfigRepository _gameplayRuleConfigRepository;
        private readonly IMapper _mapper;
        public GetGameplayRuleConfigsQueryHandler(IGameplayRuleConfigRepository gameplayRuleConfigRepository, IMapper mapper)
        {
            _gameplayRuleConfigRepository = gameplayRuleConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<GameplayRuleConfigsModel>>> Handle(GetGameplayRuleConfigsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<GameplayRuleConfigsModel>>();

            var gameplayRuleConfigModels = await _gameplayRuleConfigRepository.Queryable.OrderBy(p => p.StartRoundNumber).ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map<IList<GameplayRuleConfigsModel>>(gameplayRuleConfigModels);
            return methodResult;
        }
    }
}

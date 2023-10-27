// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.GameplayTimeConfigs
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.GameplayTimeConfigs;
    using Fsel.Common.ActionResults;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetGameplayTimeConfigsQuery : IRequest<MethodResult<IList<GameplayTimeConfigsModel>>>
    {
    }

    public class GetGameplayTimeConfigsQueryHandler : IRequestHandler<GetGameplayTimeConfigsQuery, MethodResult<IList<GameplayTimeConfigsModel>>>
    {
        private readonly IGameplayTimeConfigRepository _gameplayTimeConfigRepository;

        public GetGameplayTimeConfigsQueryHandler(IGameplayTimeConfigRepository gameplayTimeConfigRepository)
        {
            _gameplayTimeConfigRepository = gameplayTimeConfigRepository;
        }

        public async Task<MethodResult<IList<GameplayTimeConfigsModel>>> Handle(GetGameplayTimeConfigsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<GameplayTimeConfigsModel>>();

            var gameplayTimeConfigsModel = await _gameplayTimeConfigRepository.Queryable.GroupBy(p => p.RoundNumber).Select(p => new GameplayTimeConfigsModel
            {
                RoundNumber = p.Key,
                GameplayTimeConfigs = p.Select(n => new GameplayTimeConfigModel
                {
                    Id = n.Id,
                    RoundNumber = n.RoundNumber,
                    GameVocabPDType = n.GameVocabPDType,
                    Time = n.Time,
                    Percent = n.Percent,
                }).ToList(),
            }).OrderBy(m => m.RoundNumber).ToListAsync(cancellationToken);

            methodResult.Result = gameplayTimeConfigsModel;
            return methodResult;
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.GameHistoryQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using MediatR;

    public class SearchGameHistoryQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<GameHistoryModel>>>
    {
    }

    public class SearchGameHistoryQueryHandler : IRequestHandler<SearchGameHistoryQuery, MethodResult<PagingItemsModel<GameHistoryModel>>>
    {
        private readonly IGameHistoryRepository _gameHistoryRepository;

        public SearchGameHistoryQueryHandler(IGameHistoryRepository gameHistoryRepository)
        {
            _gameHistoryRepository = gameHistoryRepository;
        }

        public async Task<MethodResult<PagingItemsModel<GameHistoryModel>>> Handle(SearchGameHistoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var gameHistory = _gameHistoryRepository.Queryable;
            return await _gameHistoryRepository.GetListByPageResultAsync<GameHistoryModel>(gameHistory, request, cancellationToken).ConfigureAwait(false);
        }
    }
}

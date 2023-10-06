// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.QuestBankQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Application.Services.SystemServices;
    using Fsel.Cms.PlanetDefender.Application.Services.SystemServices.Models;
    using Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.QuestBanks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SearchAllQuestBankQuery : SearchGameVocabularyQueryModel, IRequest<MethodResult<PagingItemsModel<GameVocabularyModel>>>
    {
    }

    public class SearchAllQuestBankQueryHandler : IRequestHandler<SearchAllQuestBankQuery, MethodResult<PagingItemsModel<GameVocabularyModel>>>
    {
        private readonly ISystemService _systemService;

        public SearchAllQuestBankQueryHandler(ISystemService systemService)
        {
            _systemService = systemService;
        }

        public async Task<MethodResult<PagingItemsModel<GameVocabularyModel>>> Handle(SearchAllQuestBankQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<GameVocabularyModel>> methodResult = new MethodResult<PagingItemsModel<GameVocabularyModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var gameVocabularyResult = await _systemService.SearchGameVocabularyAsync(request);

            var gameVocabulary = gameVocabularyResult.Content?.Result;

            methodResult.Result = gameVocabulary;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}

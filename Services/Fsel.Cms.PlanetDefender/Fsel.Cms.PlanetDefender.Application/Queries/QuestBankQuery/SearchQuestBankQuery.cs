// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.QuestBankQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Application.Services.SystemServices;
    using Fsel.Cms.PlanetDefender.Application.Services.SystemServices.Models;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SearchQuestBankQuery : SearchGameVocabularyQueryModel, IRequest<MethodResult<PagingItemsModel<GameVocabularyModel>>>
    {
    }

    public class SearchQuestBankQueryHandler : IRequestHandler<SearchQuestBankQuery, MethodResult<PagingItemsModel<GameVocabularyModel>>>
    {
        private readonly IQuestBankRepository _questBankRepository;
        private readonly ISystemService _systemService;

        public SearchQuestBankQueryHandler(IQuestBankRepository questBankRepository, ISystemService systemService)
        {
            _questBankRepository = questBankRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<PagingItemsModel<GameVocabularyModel>>> Handle(SearchQuestBankQuery request, CancellationToken cancellationToken)
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

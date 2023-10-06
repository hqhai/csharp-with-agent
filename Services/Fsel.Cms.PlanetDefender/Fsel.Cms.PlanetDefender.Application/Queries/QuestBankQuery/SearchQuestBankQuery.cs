// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.QuestBankQuery
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Application.Services.SystemServices;
    using Fsel.Cms.PlanetDefender.Application.Services.SystemServices.Models;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.QuestBanks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SearchQuestBankQuery : SearchQuestBankQueryModel, IRequest<MethodResult<PagingItemsModel<GameVocabularyModel>>>
    {
    }

    public class SearchQuestBankQueryHandler : IRequestHandler<SearchQuestBankQuery, MethodResult<PagingItemsModel<GameVocabularyModel>>>
    {
        private readonly ISystemService _systemService;
        private readonly IQuestBankRepository _questBankRepository;
        private readonly IMapper _mapper;
        public SearchQuestBankQueryHandler(ISystemService systemService, IQuestBankRepository questBankRepository, IMapper mapper)
        {
            _systemService = systemService;
            _questBankRepository = questBankRepository;
            _mapper = mapper;
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
            var questBank = _questBankRepository.Queryable.Where(x => x.IsActive).Select(x => new QuestBankModel
            {
                Id = x.Id,
                CreatedDate = x.CreatedDate,
                GameVocabularyId = x.GameVocabularyId,
                IsActive = x.IsActive,
            });
            var a = _mapper.Map<SearchGameVocabularyQueryModel>(request);
            a.GameVocabularyIds = questBank.Select(p => p.GameVocabularyId).ToList();
            var gameVocabularyResult = await _systemService.SearchGameVocabularyAsync(a);
            var gameVocabulary = gameVocabularyResult.Content?.Result;

            /*int totalItem = await questBank.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await questBank
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);*/
            /*methodResult.Result = new PagingItemsModel<GameVocabularyModel>(lists, request, totalItem);*/
            methodResult.Result = gameVocabulary;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}

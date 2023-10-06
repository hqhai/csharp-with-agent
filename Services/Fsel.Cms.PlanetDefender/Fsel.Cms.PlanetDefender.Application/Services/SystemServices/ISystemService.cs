// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Services.SystemServices
{
    using Fsel.Cms.PlanetDefender.Application.Services.SystemServices.Models;
    using Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.QuestBanks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Refit;

    public interface ISystemService
    {
        [Post("/game-vocabulary/search")]
        Task<IApiResponse<MethodResult<PagingItemsModel<GameVocabularyModel>>>> SearchGameVocabularyAsync([Body] SearchGameVocabularyQueryModel model);
    }
}

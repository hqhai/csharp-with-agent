// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Services.SystemServices
{
    using Fsel.Cms.PlanetDefender.Application.Services.SystemServices.Models;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Refit;

    public interface ISystemService
    {
        [Post("/game-vocabulary/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<GameVocabularyModel>>>> ExecuteListGameVocabularyQueryAsync([Body] BaseQueryModel query);
    }
}

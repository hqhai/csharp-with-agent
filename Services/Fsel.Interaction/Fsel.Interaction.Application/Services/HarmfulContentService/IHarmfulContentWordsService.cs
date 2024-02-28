// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.HarmfulContentService
{
    using Fsel.Interaction.Application.Services.HarmfulContentService.Models;
    using Refit;

    public interface IHarmfulContentWordsService
    {
        [Post("/v1/text:analyze")]
        Task<IApiResponse<HarmfulContentWordsModel>> CheckHarmfulContentWords([Body] CheckHarmfulContentWordsModel model, [AliasAs("api-version")] string? version);
    }
}

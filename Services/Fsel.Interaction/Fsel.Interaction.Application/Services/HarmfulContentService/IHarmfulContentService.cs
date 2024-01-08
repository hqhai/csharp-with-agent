// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.HarmfulContentService
{
    using Fsel.Interaction.Application.Services.HarmfulContentService.Models;
    using Refit;

    public interface IHarmfulContentService
    {
        [Post("/text:analyze")]
        Task<IApiResponse<HarmfulContentModel>> CheckHarmfulContentWords([Body] CheckHarmfulContentWordsModel model, [AliasAs("api-version")] string? version);

        [Post("/image:analyze")]
        Task<IApiResponse<HarmfulContentModel>> CheckHarmfulContentImage([Body] CheckHarmfulContentImagesModel model, [AliasAs("api-version")] string? version);
    }
}

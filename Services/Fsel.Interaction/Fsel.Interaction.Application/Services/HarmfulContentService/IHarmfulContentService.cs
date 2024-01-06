// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.HarmfulContentService
{
    using Fsel.Interaction.Application.Services.HarmfulContentService.Models;
    using Refit;

    public interface IHarmfulContentService
    {
        [Post("/text:analyze")]
        Task<IApiResponse<HarmfulContentModel>> CheckHarmfulContentWords([Body] CheckHarmfulContentWordsModel model, [Header("Ocp-Apim-Subscription-Key")] string? subscriptionKey, [Header("Content-Type")] string? contentType, [AliasAs("api-version")] string? version);

        [Post("/image:analyze")]
        Task<IApiResponse<HarmfulContentModel>> CheckHarmfulContentImage([Body] CheckHarmfulContentImagesModel model, [Header("Ocp-Apim-Subscription-Key")] string? subscriptionKey, [Header("Content-Type")] string? contentType, [AliasAs("api-version")] string? version);
    }
}

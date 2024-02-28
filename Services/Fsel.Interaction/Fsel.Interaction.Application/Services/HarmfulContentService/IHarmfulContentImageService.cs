// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.HarmfulContentService
{
    using System.Threading.Tasks;
    using Fsel.Interaction.Application.Services.HarmfulContentService.Models;
    using Refit;

    public interface IHarmfulContentImageService
    {
        [Post("/Evaluate")]
        Task<IApiResponse<HarmfulContentImageModel>> CheckHarmfulContentImage([Body] CheckHarmfulContentImagesModel model, [AliasAs("CacheImage")] bool cacheImage);
    }
}

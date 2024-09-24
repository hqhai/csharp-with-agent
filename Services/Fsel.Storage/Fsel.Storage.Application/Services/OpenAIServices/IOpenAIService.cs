// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Services.OpenAIServices
{
    using Fsel.Storage.Application.Services.OpenAIServices.Models;
    using Refit;

    public interface IOpenAIService
    {
        [Post("/v1/audio/speech")]
        Task<IApiResponse<HttpContent>> GenerateAudioByAIAsync([Body] AudioChatbotModel command);
    }
}

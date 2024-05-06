// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.StorageServices
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Application.Services.StorageServices.Models;
    using Refit;

    public interface IStorageService
    {
        [Post("/v1/transcript/chatbot-speech")]
        Task<IApiResponse<MethodResult<string>>> TextToSpeech([Body] CreateChatbotAudioModel command);
    }
}

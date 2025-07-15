// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Services.OpenAIServices
{
    using Fsel.Storage.Application.Services.OpenAIServices.Models;
    using Refit;

    public interface IOpenAIService
    {
        [Post("/v1/audio/speech")]
        Task<IApiResponse<HttpContent>> GenerateAudioByAIAsync([Body] AudioChatbotModel command);

        [Multipart]
        [Post("/v1/audio/transcriptions")]
        Task<IApiResponse<string>> SpeechToTextByAIAsync([AliasAs("file")] StreamPart? file, [AliasAs("model")] string? model);

        [Multipart]
        [Post("/v1/audio/transcriptions")]
        Task<IApiResponse<string>> SpeechToTextByAISetLanguageAsync([AliasAs("file")] StreamPart? file, [AliasAs("model")] string? model, [AliasAs("language")] string? language);
    }
}

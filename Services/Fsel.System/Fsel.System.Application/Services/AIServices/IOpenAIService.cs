// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.AIServices
{
    using Fsel.System.Application.Services.AIServices.Models;
    using Refit;

    public interface IOpenAIService
    {
        [Post("/v1/audio/speech")]
        Task<IApiResponse<HttpContent>> GenerateAudioByAIAsync([Body] AudioChatbotModel command);

        [Post("/v1/chat/completions")]
        Task<IApiResponse<AIResponseModel>> SubmitAICompletionsAsync([Body] RequestAIModel command);

        [Post("/v1/embeddings")]
        Task<IApiResponse<EmbeddingResponseModel>> GenerateEmbeddingAsync([Body] EmbeddingRequest request);
    }
}

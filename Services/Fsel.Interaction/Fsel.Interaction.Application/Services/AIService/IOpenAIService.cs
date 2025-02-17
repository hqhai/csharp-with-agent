// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.AIService
{
    using Fsel.Interaction.Application.Services.AIService.Models;
    using Refit;

    public interface IOpenAIService
    {
        [Post("/v1/chat/completions")]
        Task<IApiResponse<AIResponseModel>> SubmitAICompletionsAsync([Body] RequestAIModel command);
    }
}

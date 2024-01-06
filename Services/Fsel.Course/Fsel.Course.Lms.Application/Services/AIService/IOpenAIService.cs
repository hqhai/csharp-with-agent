// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.AiService
{
    using Fsel.Course.Lms.Application.Services.AiService.Models;
    using Fsel.Course.Lms.Application.Services.AIService.Models;
    using Refit;

    public interface IOpenAIService
    {
        [Post("/chat/completions")]
        Task<IApiResponse<AIResponseModel>> SubmitAICompletionsAsync([Body] RequestAIModel command);
    }
}

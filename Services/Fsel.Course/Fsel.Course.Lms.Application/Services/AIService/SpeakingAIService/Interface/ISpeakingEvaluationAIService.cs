// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService.Interface
{
    public interface ISpeakingEvaluationAIService
    {
        Task<double> EvaluationSpeaking(string? question, string? url);

        Task<double> EvaluationSpeakingV1(string? question, string? url);
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Services.AIService.SpeakingAIService.Interface
{
    public interface ISpeakingEvaluationAIService
    {
        Task<double> EvaluationSpeaking(string? question, string? url);
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Services.AiService.SpeakingAIService
{
    public interface ISpeakingAIService
    {
        Task<bool> EvaluationSpeakingAI(Guid examPracticeResultId, Guid examPracticeSectionId, CancellationToken cancellationToken);
    }
}

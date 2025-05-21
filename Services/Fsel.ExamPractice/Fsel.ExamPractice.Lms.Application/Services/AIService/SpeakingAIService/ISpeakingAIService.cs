// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Services.AiService.SpeakingAIService
{
    public interface ISpeakingAIService
    {
        Task<bool> EvaluationSpeakingAI(Guid mockTestResultId, Guid sectionGroupId, CancellationToken cancellationToken);
    }
}

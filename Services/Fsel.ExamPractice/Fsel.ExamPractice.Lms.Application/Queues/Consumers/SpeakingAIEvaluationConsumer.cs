// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.ExamPractice.Lms.Application.Services.AiService.SpeakingAIService;
    using Fsel.Shared.Models.ShareModels;

    public class SpeakingAIEvaluationConsumer : BaseConsumer<SpeakingExamPracticeAIEvaluationModel>
    {
        private readonly ISpeakingAIService _speakingAIService;

        public SpeakingAIEvaluationConsumer(AuthContext authContext, ISpeakingAIService speakingAIService, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _speakingAIService = speakingAIService;
        }

        public override async Task ConsumeQueue(SpeakingExamPracticeAIEvaluationModel? message)
        {
            if (message != null)
            {
                await _speakingAIService.EvaluationSpeakingAI(message.ExamPracticeResultId, message.ExamPracticeSectionId, CancellationToken.None).ConfigureAwait(false);
            }
        }
    }
}

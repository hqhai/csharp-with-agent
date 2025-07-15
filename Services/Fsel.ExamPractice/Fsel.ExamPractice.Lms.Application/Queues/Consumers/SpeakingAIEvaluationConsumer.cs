// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.ExamPractice.Lms.Application.Services.AiService.SpeakingAIService;
    using Fsel.Shared.Models.ShareModels;

    public class SpeakingAIEvaluationConsumer : BaseConsumer<SpeakingAIEvaluationModel>
    {
        private readonly ISpeakingAIService _speakingAIService;

        public SpeakingAIEvaluationConsumer(AuthContext authContext, ISpeakingAIService speakingAIService, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _speakingAIService = speakingAIService;
        }

        public override async Task ConsumeQueue(SpeakingAIEvaluationModel? message)
        {
            if (message != null)
            {
                await _speakingAIService.EvaluationSpeakingAI(message.MockTestResultId, message.SectionGroupId, CancellationToken.None).ConfigureAwait(false);
            }
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class SubmitSpeakingAIPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public SubmitSpeakingAIPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(SpeakingExamPracticeAIEvaluationModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.ExamPracticeQueue.NameQueue.SpeakingAI, request, cancellationToken);
        }
    }
}

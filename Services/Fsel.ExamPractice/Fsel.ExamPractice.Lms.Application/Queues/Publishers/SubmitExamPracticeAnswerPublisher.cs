// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPracticeAnswers;
    using Fsel.Shared.Constants;

    public class SubmitExamPracticeAnswerPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public SubmitExamPracticeAnswerPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(ExamPracticeAnswerResponseModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.ExamPracticeQueue.NameQueue.ExamPracticeAnwserResponse, request, cancellationToken);
        }
    }
}

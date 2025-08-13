// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Publishers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models;

    public class TracingQuestionTypePublisher
    {
        private readonly IQueueProvider _queueProvider;

        public TracingQuestionTypePublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(QuestionResultQueueModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.RealtimeQueue.NameQueue.TracingQuestionType, request, cancellationToken);
        }
    }
}

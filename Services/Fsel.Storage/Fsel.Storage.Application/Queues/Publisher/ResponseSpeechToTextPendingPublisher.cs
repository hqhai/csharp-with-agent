// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Queues.Publisher
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class ResponseSpeechToTextPendingPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public ResponseSpeechToTextPendingPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(ResponseSpeechToTextPendingAiConsumerModel request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.StorageQueue.NameQueue.ResponseSpeechToTextPendingAi, request, cancellationToken);
        }
    }
}

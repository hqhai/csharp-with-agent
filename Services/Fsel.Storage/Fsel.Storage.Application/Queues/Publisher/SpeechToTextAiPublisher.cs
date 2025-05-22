// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Queues.Publisher
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class SpeechToTextAiPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public SpeechToTextAiPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(SpeechToTextAiConsumerModel request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.StorageQueue.NameQueue.SpeechToTextAi, request, cancellationToken);
        }
    }
}

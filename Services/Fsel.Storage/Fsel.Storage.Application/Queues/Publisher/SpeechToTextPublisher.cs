// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Queues.Publisher
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class SpeechToTextPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public SpeechToTextPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(SpeechToTextConsumerModel request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.RealtimeQueue.NameQueue.SpeechToTextRealTime, request, cancellationToken);
        }
    }
}

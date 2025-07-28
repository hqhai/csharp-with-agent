// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Publishers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;

    public class DictionaryPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public DictionaryPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(string? word, CancellationToken cancellationToken)
        {
            if (word == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.RealtimeQueue.NameQueue.DictionaryRealTime, word, cancellationToken);
        }
    }
}

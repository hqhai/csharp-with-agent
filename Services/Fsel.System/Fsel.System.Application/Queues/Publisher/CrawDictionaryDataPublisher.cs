// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queues.Publisher
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;

    public class CrawDictionaryDataPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public CrawDictionaryDataPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(string? message, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.SystemQueue.NameQueue.CrawDictionaryData, message, cancellationToken).ConfigureAwait(false);
        }
    }
}

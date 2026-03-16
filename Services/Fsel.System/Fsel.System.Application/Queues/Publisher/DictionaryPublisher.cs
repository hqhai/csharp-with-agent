// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queues.Publisher
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class DictionaryPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public DictionaryPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(DictionaryQueueModel? message, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.SystemQueue.NameQueue.SendDictionary, message, cancellationToken).ConfigureAwait(false);
        }

        public async Task PublishSemanticDictionary(SemanticDictionaryQueueModel? message, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.SystemQueue.NameQueue.SendDictionary, message, cancellationToken).ConfigureAwait(false);
        }
    }
}

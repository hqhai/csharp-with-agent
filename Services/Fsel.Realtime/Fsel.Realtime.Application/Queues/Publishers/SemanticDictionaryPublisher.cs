// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Publishers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class SemanticDictionaryPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public SemanticDictionaryPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(string userId, SemanticDictionaryRequestModel request, CancellationToken cancellationToken)
        {
            var queueModel = new SemanticDictionaryAIQueueModel
            {
                UserId = userId,
                Request = request
            };

            await _queueProvider.Publish(
                QueueSettings.RealtimeQueue.NameQueue.SemanticDictionary,
                queueModel,
                cancellationToken);
        }
    }
}

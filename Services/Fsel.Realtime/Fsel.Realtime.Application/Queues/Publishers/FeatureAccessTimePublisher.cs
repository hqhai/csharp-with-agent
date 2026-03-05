// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Publishers

{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class FeatureAccessTimePublisher
    {
        private readonly IQueueProvider _queueProvider;

        public FeatureAccessTimePublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(TrackingTimeModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.RealtimeQueue.NameQueue.FeatureAccessTime, request, cancellationToken);
        }
    }
}

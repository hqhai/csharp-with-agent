// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Publishers
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;

    public class UpdateClassForumResultToExpiredTimePublisher
    {
        private readonly IQueueProvider _queueProvider;

        public UpdateClassForumResultToExpiredTimePublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(BaseQueueModel data, CancellationToken cancellationToken)
        {
            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.UpdateClassForumResultToExpiredTime, data, cancellationToken);
        }
    }
}

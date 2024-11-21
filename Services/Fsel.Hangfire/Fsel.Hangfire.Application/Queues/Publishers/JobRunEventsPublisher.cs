// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Publishers
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;

    public class JobRunEventsPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public JobRunEventsPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(CancellationToken cancellationToken)
        {
            await _queueProvider.Publish(QueueSettings.UserQueue.NameQueue.JobRunEvents, new BaseQueueModel { QueueId = Guid.NewGuid().ToString() }, cancellationToken);
        }
    }
}

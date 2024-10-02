// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Constants;

namespace Fsel.Hangfire.Application.Queues.Publishers
{
    public class NoticeAccessTimePublisher
    {
        private readonly IQueueProvider _queueProvider;

        public NoticeAccessTimePublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(CancellationToken cancellationToken)
        {
            await _queueProvider.Publish(QueueSettings.SystemQueue.NameQueue.NoticeAccessTime, new BaseQueueModel { QueueId = Guid.NewGuid().ToString() }, cancellationToken);
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Constants;

namespace Fsel.Hangfire.Application.Queues.Publishers
{
    public class LeaderBoardPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public LeaderBoardPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(CancellationToken cancellationToken)
        {
            await _queueProvider.Publish(QueueSettings.UserQueue.NameQueue.LeaderBoard, new BaseQueueModel { QueueId = Guid.NewGuid().ToString() }, cancellationToken);
        }
    }
}

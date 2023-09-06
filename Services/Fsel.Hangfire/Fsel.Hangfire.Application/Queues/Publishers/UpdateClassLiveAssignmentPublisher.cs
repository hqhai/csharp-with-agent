// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Constants;

namespace Fsel.Hangfire.Application.Queues.Publishers
{
    public class UpdateClassLiveAssignmentPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public UpdateClassLiveAssignmentPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(CancellationToken cancellationToken)
        {
            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.UpdateClassLiveAssignment, new BaseQueueModel { QueueId = Guid.NewGuid().ToString() }, cancellationToken);
        }
    }
}

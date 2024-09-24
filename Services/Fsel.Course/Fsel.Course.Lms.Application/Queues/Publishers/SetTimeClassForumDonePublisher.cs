// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;

    public class SetTimeClassForumDonePublisher
    {
        private readonly IQueueProvider _queueProvider;

        public SetTimeClassForumDonePublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(BaseQueueModel baseQueue, CancellationToken cancellationToken)
        {
            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.SetTimeClassForumDone, baseQueue, cancellationToken);
        }
    }
}

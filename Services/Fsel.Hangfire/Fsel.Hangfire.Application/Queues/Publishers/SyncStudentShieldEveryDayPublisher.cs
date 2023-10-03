// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Publishers
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;

    public class SyncStudentShieldEveryDayPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public SyncStudentShieldEveryDayPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(CancellationToken cancellationToken)
        {
            await _queueProvider.Publish(QueueSettings.UserQueue.NameQueue.SyncStudentShieldEveryDay, cancellationToken);
        }
    }
}

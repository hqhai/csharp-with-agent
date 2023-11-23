// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class CompleteTestWhenTimeOutPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public CompleteTestWhenTimeOutPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(CompleteTestWhenTimeOutModel data, CancellationToken cancellationToken)
        {
            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.CompleteTestWhenTimeOut, data, cancellationToken);
        }
    }
}

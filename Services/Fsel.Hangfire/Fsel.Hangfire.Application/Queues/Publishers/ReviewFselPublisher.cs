// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;

namespace Fsel.Hangfire.Application.Queues.Publishers
{
    public class ReviewFselPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public ReviewFselPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(NotificationSendingQueueModel? data, CancellationToken cancellationToken)
        {
            await _queueProvider.Publish(QueueSettings.SystemQueue.NameQueue.ReviewFsel, data, cancellationToken);
        }
    }
}

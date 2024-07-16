// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queues.Publishers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class ChangeStatusOrderPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public ChangeStatusOrderPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(OrderQueueModel order, CancellationToken cancellationToken)
        {
            if (order == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.OrderingQueue.NameQueue.ChangeStatusOrder, order, cancellationToken);
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class CreateOrderPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public CreateOrderPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(CreateOrderQueueModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.OrderCreateNotification, request, cancellationToken);
        }
    }
}

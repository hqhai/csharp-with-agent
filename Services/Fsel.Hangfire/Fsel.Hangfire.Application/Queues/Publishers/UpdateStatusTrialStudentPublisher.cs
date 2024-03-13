// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class UpdateStatusTrialStudentPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public UpdateStatusTrialStudentPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }
        public async Task Publish(CancellationToken cancellationToken)
        {
            await _queueProvider.Publish(QueueSettings.OrderingQueue.NameQueue.NoticePayment, cancellationToken);
        }
    }
}

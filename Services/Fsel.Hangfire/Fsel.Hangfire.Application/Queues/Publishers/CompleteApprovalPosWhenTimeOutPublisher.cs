// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class CompleteApprovalPosWhenTimeOutPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public CompleteApprovalPosWhenTimeOutPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(SetTimeCompleteApprovalModel data, CancellationToken cancellationToken)
        {
            await _queueProvider.Publish(QueueSettings.SystemQueue.NameQueue.CompleteApprovalPostTimeOut, data, cancellationToken);
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queues.Publishers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class CompleteApprovalPostPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public CompleteApprovalPostPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(SetTimeCompleteApprovalModel request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.SystemQueue.NameQueue.SetCompleteApprovalPost, request, cancellationToken);
        }
    }
}

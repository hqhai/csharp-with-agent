// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queues.Publisher
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using global::System.Threading.Tasks;


    public class SetCompleteApprovalPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public SetCompleteApprovalPublisher(IQueueProvider queueProvider)
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

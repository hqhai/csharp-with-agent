// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class TokenHistoryPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public TokenHistoryPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(TokenHistoryQueueModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.UserQueue.NameQueue.CreateTokenHistory, request, cancellationToken);
        }
    }
}

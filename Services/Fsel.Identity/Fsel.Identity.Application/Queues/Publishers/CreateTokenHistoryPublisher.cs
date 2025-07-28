// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class CreateTokenHistoryPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public CreateTokenHistoryPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(IList<TokenHistoryQueueModel>? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.UserQueue.NameQueue.CreateTokenHistory, new TokenHistoryQueuesModel { TokenHistories = request }, cancellationToken);
        }
    }
}

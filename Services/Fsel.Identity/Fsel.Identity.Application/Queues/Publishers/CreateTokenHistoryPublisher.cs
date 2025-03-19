// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.Extensions.Logging;

    public class CreateTokenHistoryPublisher
    {
        private readonly IQueueProvider _queueProvider;
        private readonly ILogger<CreateTokenHistoryPublisher> _logger;

        public CreateTokenHistoryPublisher(IQueueProvider queueProvider, ILogger<CreateTokenHistoryPublisher> logger)
        {
            _queueProvider = queueProvider;
            _logger = logger;
        }

        public async Task Publish(IList<TokenHistoryQueueModel>? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            _logger.LogError("CreateTokenHistory");
            await _queueProvider.Publish(QueueSettings.UserQueue.NameQueue.CreateTokenHistory, new TokenHistoryQueuesModel { TokenHistories = request }, cancellationToken);
        }
    }
}

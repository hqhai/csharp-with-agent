// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Publishers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.Extensions.Logging;

    public class AITranslationRequestPublisher
    {
        private readonly IQueueProvider _queueProvider;
        private readonly ILogger<AITranslationRequestPublisher> _logger;

        private const string QueueName = QueueSettings.LmsQueue.NameQueue.AITranslationResponse;

        public AITranslationRequestPublisher(IQueueProvider queueProvider, ILogger<AITranslationRequestPublisher> logger)
        {
            _queueProvider = queueProvider;
            _logger = logger;
        }

        public async Task Publish(AITranslationRequestModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueName, request, cancellationToken);

        }

    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queues.Publisher
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.Extensions.Logging;

    public class ChatBotPublisher
    {
        private readonly IQueueProvider _queueProvider;
        private readonly ILogger<ChatBotPublisher> _logger;

        public ChatBotPublisher(IQueueProvider queueProvider, ILogger<ChatBotPublisher> logger)
        {
            _queueProvider = queueProvider;
            _logger = logger;
        }

        public async Task Publish(ChatBotSendingMessageModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            _logger.LogInformation($"Publisher Send ChatbotWS:{ConvertHelper.Serialize(request)}, Environment.MachineName: {Environment.MachineName}");
            await _queueProvider.Publish(QueueSettings.RealtimeQueue.NameQueue.ChatBotRealTime, request, cancellationToken);
        }
    }
}

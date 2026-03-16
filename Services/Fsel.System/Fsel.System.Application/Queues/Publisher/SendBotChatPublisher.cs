// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queues.Publisher
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.Extensions.Logging;

    public class SendBotChatPublisher
    {
        private readonly IQueueProvider _queueProvider;
        private readonly ILogger<SendBotChatPublisher> _logger;

        public SendBotChatPublisher(IQueueProvider queueProvider, ILogger<SendBotChatPublisher> logger)
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
            await _queueProvider.Publish(QueueSettings.SystemQueue.NameQueue.SendBotChat, request, cancellationToken);
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queues.Publisher
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class ChatBotPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public ChatBotPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(ChatBotSendingMessageModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.RealtimeQueue.NameQueue.ChatBotRealTime, request, cancellationToken);
        }
    }
}

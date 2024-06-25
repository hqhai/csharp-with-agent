using Fsel.Core.Base;
using Fsel.Core.Extensions;
using Fsel.Realtime.Application.Hubs;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Fsel.Realtime.Application.Queues.Consumers
{
    public class ChatBotConsumer : BaseConsumer<ChatBotSendingMessageModel>
    {
        private readonly IHubContext<ChatBotHub> _chatBotHubContext;

        public ChatBotConsumer(IHubContext<ChatBotHub> chatBotHubContext, AuthContext authContext) : base(authContext)
        {
            _chatBotHubContext = chatBotHubContext;
        }

        public override async Task ConsumeQueue(ChatBotSendingMessageModel? message)
        {
            if (message != null)
            {
                var chatBotId = message.ChatbotId.ToString();
                await _chatBotHubContext.GetGroup(chatBotId!).SendAsync(RealtimeSettings.ChatBotHub.Methods.ChatBot, message);
            }
        }
    }
}

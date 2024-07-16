using Fsel.Common.Helpers;
using Fsel.Core.Base;
using Fsel.Core.Extensions;
using Fsel.Realtime.Application.Hubs;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Fsel.Realtime.Application.Queues.Consumers
{
    public class ChatBotConsumer : BaseConsumer<ChatBotSendingMessageModel>
    {
        private readonly IHubContext<ChatBotHub> _chatBotHubContext;
        private readonly ILogger<ChatBotConsumer> _logger;


        public ChatBotConsumer(IHubContext<ChatBotHub> chatBotHubContext, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor, ILogger<ChatBotConsumer> logger) : base(authContext, httpContextAccessor)
        {
            _chatBotHubContext = chatBotHubContext;
            _logger = logger;
        }

        public override async Task ConsumeQueue(ChatBotSendingMessageModel? message)
        {
            _logger.LogInformation($"Consumer Receive Chatbot Message:{ConvertHelper.Serialize(message)}, Environment.MachineName: {Environment.MachineName}");
            if (message != null)
            {
                var chatBotId = message.ChatbotId.ToString();
                _logger.LogInformation($"Consumer Send to ChatBotHub:{message.ChatbotId}, Environment.MachineName: {Environment.MachineName}");
                await _chatBotHubContext.GetGroup(chatBotId!).SendAsync(RealtimeSettings.ChatBotHub.Methods.ChatBot, message);
            }
        }
    }
}

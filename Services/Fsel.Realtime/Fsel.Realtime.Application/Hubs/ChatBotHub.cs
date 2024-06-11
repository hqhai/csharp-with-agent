using Fsel.Core.Base;
using Fsel.Core.Extensions;
using Fsel.Core.Services.IpApiServices;
using Fsel.Realtime.Application.Queues.Publishers;
using Fsel.Shared.Models.ShareModels;
using Microsoft.AspNetCore.SignalR;

// Đảm bảo rằng bạn đã thêm namespace của ConnectionTracker

namespace Fsel.Realtime.Application.Hubs
{
    public class ChatBotHub : BaseHub
    {
        private readonly AuthContext _authContext;
        private readonly ChatBotPublisher _botPublisher;

        public ChatBotHub(AuthContext authContext, IIpApiService ipApiService, ChatBotPublisher botPublisher) : base(authContext, ipApiService)
        {
            _authContext = authContext;
            _botPublisher = botPublisher;
        }

        public override async Task OnConnectedHubAsync()
        {
            string chatBotId = Context.GetHttpContext()?.Request.Query["ChatBotId"].ToString()!;

            await Groups.AddGroupAsync(Context.ConnectionId, chatBotId);
        }

        public class Config
        {
            public string? Role { get; set; }
            public string? Content { get; set; }

            public Guid? ChatBotId { get; set; }
        }

        public async Task SendMessage(Config config, string customValue)
        {
            ChatBotSendingMessageModel model = new ChatBotSendingMessageModel
            {
                ChatbotId = config?.ChatBotId,
                Content = config?.Content ?? string.Empty,
            };

            await _botPublisher.Publish(model, CancellationToken.None);
        }

        public override async Task OnDisconnectedHubAsync(Exception? exception)
        {
            string chatBotId = Context.GetHttpContext()?.Request.Query["ChatBotId"].ToString()!;

            if (!string.IsNullOrEmpty(chatBotId))
            {
                await Groups.RemoveGroupAsync(Context.ConnectionId, chatBotId);
            }
        }
    }
}

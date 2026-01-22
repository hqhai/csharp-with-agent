using Fsel.Core.Base;
using Fsel.Core.Extensions;
using Fsel.Core.Services.IpApiServices;
using Fsel.Realtime.Application.Queues.Publishers;
using Fsel.Shared.Models.ShareModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

// Đảm bảo rằng bạn đã thêm namespace của ConnectionTracker

namespace Fsel.Realtime.Application.Hubs
{
    public class ChatBotHub : BaseHub
    {
        private readonly AuthContext _authContext;
        private readonly ChatBotPublisher _botPublisher;
        private readonly ILogger<ChatBotHub> _logger;

        public ChatBotHub(AuthContext authContext, IIpApiService ipApiService, IHttpContextAccessor httpContextAccessor, ChatBotPublisher botPublisher, ILogger<ChatBotHub> logger) : base(authContext, ipApiService, httpContextAccessor)
        {
            _authContext = authContext;
            _botPublisher = botPublisher;
            _logger = logger;
        }

        public override async Task OnConnectedHubAsync()
        {
            //string chatBotId = Context.GetHttpContext()?.Request.Query["ChatBotId"].ToString()!;

            //await Groups.AddGroupAsync(Context.ConnectionId, chatBotId);
        }

        public class Config
        {
            public string? Role { get; set; }
            public string? Content { get; set; }
            public Guid? ChatBotId { get; set; }
        }

        public async Task SendMessage(Config config, string? customValue)
        {
            if (config != null && config.ChatBotId.HasValue)
            {
                await Groups.AddGroupAsync(Context.ConnectionId, config.ChatBotId.ToString() ?? string.Empty);

                ChatBotSendingMessageModel model = new ChatBotSendingMessageModel
                {
                    ChatbotId = config.ChatBotId ?? Guid.Empty,
                    Content = config.Content ?? string.Empty,
                };
                _logger.LogInformation($"Start Invoke ChatBot: Content : {config.Content}, ChatbotId: {config.ChatBotId}, Environment.MachineName: {Environment.MachineName}");
                await _botPublisher.Publish(model, CancellationToken.None);
            }
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

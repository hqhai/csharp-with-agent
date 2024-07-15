using Fsel.Common.Helpers;
using Fsel.Core.Base;
using Fsel.Core.Extensions;
using Fsel.Core.Services.IpApiServices;
using Fsel.Realtime.Application.Queues.Publishers;
using Fsel.Shared.Models.ShareModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

// Đảm bảo rằng bạn đã thêm namespace của ConnectionTracker

namespace Fsel.Realtime.Application.Hubs
{
    public class TechieHub : BaseHub
    {
        private readonly AuthContext _authContext;
        private readonly TechieActionPublisher _actionPublisher;
        private readonly ILogger<object> _logger;

        public TechieHub(AuthContext authContext, IIpApiService ipApiService, TechieActionPublisher actionPublisher, ILogger<object> logger, IHttpContextAccessor httpContextAccessor) : base(authContext, ipApiService, httpContextAccessor)
        {
            _authContext = authContext;
            _actionPublisher = actionPublisher;
            _logger = logger;
        }

        public override async Task OnConnectedHubAsync()
        {
            await Groups.AddGroupAsync(Context.ConnectionId, _authContext.CurrentUserId.ToString());
        }

        public class Config
        {
            public string? Role { get; set; }
            public string? Content { get; set; }

            public Guid? ChatBotId { get; set; }
        }

        public async Task TechieSendAction(string modelStr)
        {
            _logger.LogInformation($"Start Invoke!: {modelStr}");
            var model = ConvertHelper.Deserialize<StudentTechieActionModel>(modelStr);
            await _actionPublisher.Publish(model, CancellationToken.None);
        }

        public override async Task OnDisconnectedHubAsync(Exception? exception)
        {
            await Groups.RemoveGroupAsync(Context.ConnectionId, _authContext.CurrentUserId.ToString());
        }
    }
}

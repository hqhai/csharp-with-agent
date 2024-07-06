using Fsel.Common.Helpers;
using Fsel.Core.Base;
using Fsel.Core.Extensions;
using Fsel.Core.Services.IpApiServices;
using Fsel.Realtime.Application.Queues.Publishers;
using Fsel.Shared.Models.ShareModels;
using Microsoft.AspNetCore.SignalR;

// Đảm bảo rằng bạn đã thêm namespace của ConnectionTracker

namespace Fsel.Realtime.Application.Hubs
{
    public class TechieHub : BaseHub
    {
        private readonly AuthContext _authContext;
        private readonly TechieActionPublisher _actionPublisher;

        public TechieHub(AuthContext authContext, IIpApiService ipApiService, TechieActionPublisher actionPublisher) : base(authContext, ipApiService)
        {
            _authContext = authContext;
            _actionPublisher = actionPublisher;
        }

        public override async Task OnConnectedHubAsync()
        {
            string studentId = Context.GetHttpContext()?.Request.Query["StudentId"].ToString()!;

            await Groups.AddGroupAsync(Context.ConnectionId, studentId);
        }

        public class Config
        {
            public string? Role { get; set; }
            public string? Content { get; set; }

            public Guid? ChatBotId { get; set; }
        }

        public async Task TechieSendAction(string modelStr)
        {
            var model = ConvertHelper.Deserialize<StudentTechieActionModel>(modelStr);
            Console.WriteLine("Test");
            await _actionPublisher.Publish(model, CancellationToken.None);
        }

        public override async Task OnDisconnectedHubAsync(Exception? exception)
        {
            string chatBotId = Context.GetHttpContext()?.Request.Query["StudentId"].ToString()!;

            if (!string.IsNullOrEmpty(chatBotId))
            {
                await Groups.RemoveGroupAsync(Context.ConnectionId, chatBotId);
            }
        }
    }
}

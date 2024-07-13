// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Core.Services.IpApiServices;
    using Fsel.Realtime.Application.Trackers;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SignalR;

    public class NotificationHub : BaseHub
    {
        private readonly AuthContext _authContext;
        public NotificationHub(AuthContext authContext, IIpApiService ipApiService, IHttpContextAccessor httpContextAccessor) : base(authContext, ipApiService, httpContextAccessor)
        {
            _authContext = authContext;
        }

        public override async Task OnConnectedHubAsync()
        {
            await Groups.AddGroupAsync(Context.ConnectionId, _authContext.CurrentUserId.ToString());
            ConnectionTracker.Instance.RecordConnectionStart(Context.ConnectionId);
        }
        public override async Task OnDisconnectedHubAsync(Exception? exception)
        {
            await Groups.RemoveGroupAsync(Context.ConnectionId, _authContext.CurrentUserId.ToString());

        }

        public void Send(NotificationQueueModel? model)
        {
            Clients.All.SendAsync(RealtimeSettings.NotificationHub.Methods.NotificationMessage, model);
        }
    }
}

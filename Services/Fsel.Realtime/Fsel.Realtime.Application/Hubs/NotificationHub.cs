// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using Fsel.Core.Base;
    using Fsel.Core.Services.IpApiServices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;

    [Authorize]
    public class NotificationHub : BaseHub
    {
        private readonly AuthContext _authContext;
        public NotificationHub(AuthContext authContext, IIpApiService ipApiService) : base(authContext, ipApiService)
        {
            _authContext = authContext;
        }

        public override async Task OnConnectedHubAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, _authContext.CurrentUserId.ToString());
            ConnectionTracker.Instance.RecordConnectionStart(Context.ConnectionId);
        }
        public override async Task OnDisconnectedHubAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, _authContext.CurrentUserId.ToString());

        }

        public void Send(NotificationQueueModel? model)
        {
            Clients.All.SendAsync(RealtimeSettings.NotificationHub.Methods.NotificationMessage, model);
        }
    }
}

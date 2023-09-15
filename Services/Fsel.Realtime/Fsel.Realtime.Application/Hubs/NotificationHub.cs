// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.SignalR;

    public class NotificationHub : Hub
    {
        public void Send(NotificationQueueModel? model)
        {
            Clients.All.SendAsync(RealtimeSettings.NotificationHub.Methods.NotificationMessage, model);
        }
    }
}

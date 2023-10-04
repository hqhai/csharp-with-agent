// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Microsoft.AspNetCore.SignalR;

    public class LeaderBoardHub : BaseHub
    {
        public override async Task OnConnectedAsync()
        {
            string courseLevel = Context.GetHttpContext()?.Request.Query["CourseLevel"].ToString()!;
            if (!string.IsNullOrEmpty(courseLevel))
            {
                await Groups.AddGroupAsync(Context.ConnectionId, courseLevel);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string courseLevel = Context.GetHttpContext()?.Request.Query["CourseLevel"].ToString()!;
            if (!string.IsNullOrEmpty(courseLevel))
            {
                await Groups.RemoveGroupAsync(Context.ConnectionId, courseLevel);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}

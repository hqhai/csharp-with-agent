// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Microsoft.AspNetCore.SignalR;

    public class ClassForumAIFeedBackHub : BaseHub
    {
        public override async Task OnConnectedAsync()
        {
            string classForumResultId = Context.GetHttpContext()?.Request.Query["ClassForumResultId"].ToString()!;
            if (!string.IsNullOrEmpty(classForumResultId))
            {
                await Groups.AddGroupAsync(Context.ConnectionId, classForumResultId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string classForumResultId = Context.GetHttpContext()?.Request.Query["ClassForumResultId"].ToString()!;
            if (!string.IsNullOrEmpty(classForumResultId))
            {
                await Groups.RemoveGroupAsync(Context.ConnectionId, classForumResultId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}

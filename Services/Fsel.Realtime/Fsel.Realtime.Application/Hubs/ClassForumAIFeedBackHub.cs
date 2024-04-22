// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Core.Services.IpApiServices;
    using Microsoft.AspNetCore.SignalR;

    public class ClassForumAIFeedBackHub : BaseHub
    {
        public ClassForumAIFeedBackHub(AuthContext authContext, IIpApiService ipApiService) : base(authContext, ipApiService)
        {
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            string classForumResultId = Context.GetHttpContext()?.Request.Query["ClassForumResultId"].ToString()!;
            if (!string.IsNullOrEmpty(classForumResultId))
            {
                await Groups.AddGroupAsync(Context.ConnectionId, classForumResultId);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);

            string classForumResultId = Context.GetHttpContext()?.Request.Query["ClassForumResultId"].ToString()!;
            if (!string.IsNullOrEmpty(classForumResultId))
            {
                await Groups.RemoveGroupAsync(Context.ConnectionId, classForumResultId);
            }
        }
    }
}

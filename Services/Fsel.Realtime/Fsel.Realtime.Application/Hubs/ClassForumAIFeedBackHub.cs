// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Core.Services.IpApiServices;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SignalR;

    [Authorize]
    public class ClassForumAIFeedBackHub : BaseHub
    {
        public ClassForumAIFeedBackHub(AuthContext authContext, IHttpContextAccessor httpContextAccessor, IIpApiService ipApiService) : base(authContext, ipApiService, httpContextAccessor)
        {
        }

        public override async Task OnConnectedHubAsync()
        {
            string classForumResultId = Context.GetHttpContext()?.Request.Query["ClassForumResultId"].ToString()!;
            if (!string.IsNullOrEmpty(classForumResultId))
            {
                await Groups.AddGroupAsync(Context.ConnectionId, classForumResultId);
            }
        }

        public override async Task OnDisconnectedHubAsync(Exception? exception)
        {
            string classForumResultId = Context.GetHttpContext()?.Request.Query["ClassForumResultId"].ToString()!;
            if (!string.IsNullOrEmpty(classForumResultId))
            {
                await Groups.RemoveGroupAsync(Context.ConnectionId, classForumResultId);
            }
        }
    }
}

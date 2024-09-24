// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Core.Services.IpApiServices;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SignalR;

    public class LeaderBoardHub : BaseHub
    {
        public LeaderBoardHub(AuthContext authContext, IIpApiService ipApiService, IHttpContextAccessor httpContextAccessor) : base(authContext, ipApiService, httpContextAccessor)
        {
        }

        public override async Task OnConnectedHubAsync()
        {
            string courseLevel = Context.GetHttpContext()?.Request.Query["CourseLevel"].ToString()!;
            if (!string.IsNullOrEmpty(courseLevel))
            {
                await Groups.AddGroupAsync(Context.ConnectionId, courseLevel);
            }
        }

        public override async Task OnDisconnectedHubAsync(Exception? exception)
        {
            string courseLevel = Context.GetHttpContext()?.Request.Query["CourseLevel"].ToString()!;
            if (!string.IsNullOrEmpty(courseLevel))
            {
                await Groups.RemoveGroupAsync(Context.ConnectionId, courseLevel);
            }
        }
    }
}

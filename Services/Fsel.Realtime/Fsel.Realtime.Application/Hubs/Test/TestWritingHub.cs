// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs.Test
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Core.Services.IpApiServices;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SignalR;

    public class TestWritingHub : BaseHub
    {
        public TestWritingHub(AuthContext authContext, IIpApiService ipApiService, IHttpContextAccessor httpContextAccessor) : base(authContext, ipApiService, httpContextAccessor)
        {
        }

        public override async Task OnConnectedHubAsync()
        {
            string testResultId = Context.GetHttpContext()?.Request.Query["TestResultId"].ToString()!;
            if (!string.IsNullOrEmpty(testResultId))
            {
                await Groups.AddGroupAsync(Context.ConnectionId, testResultId);
            }
        }

        public override async Task OnDisconnectedHubAsync(Exception? exception)
        {
            string testResultId = Context.GetHttpContext()?.Request.Query["TestResultId"].ToString()!;
            if (!string.IsNullOrEmpty(testResultId))
            {
                await Groups.RemoveGroupAsync(Context.ConnectionId, testResultId);
            }
        }
    }
}

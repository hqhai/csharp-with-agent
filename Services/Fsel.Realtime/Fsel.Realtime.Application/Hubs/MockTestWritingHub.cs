// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Core.Services.IpApiServices;
    using Microsoft.AspNetCore.SignalR;

    public class MockTestWritingHub : BaseHub
    {
        public MockTestWritingHub(AuthContext authContext, IIpApiService ipApiService) : base(authContext, ipApiService)
        {
        }

        public override async Task OnConnectedAsync()
        {
            string mockTestCriteria = Context.GetHttpContext()?.Request.Query["MockTestResultId"].ToString()!;
            if (!string.IsNullOrEmpty(mockTestCriteria))
            {
                await Groups.AddGroupAsync(Context.ConnectionId, mockTestCriteria);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string mockTestCriteria = Context.GetHttpContext()?.Request.Query["MockTestResultId"].ToString()!;
            if (!string.IsNullOrEmpty(mockTestCriteria))
            {
                await Groups.RemoveGroupAsync(Context.ConnectionId, mockTestCriteria);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}

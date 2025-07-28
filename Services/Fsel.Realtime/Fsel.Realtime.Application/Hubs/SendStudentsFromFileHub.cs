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
    public class SendStudentsFromFileHub : BaseHub
    {
        public SendStudentsFromFileHub(AuthContext authContext, IIpApiService ipApiService, IHttpContextAccessor httpContextAccessor) : base(authContext, ipApiService, httpContextAccessor)
        {
        }

        public override async Task OnConnectedHubAsync()
        {
            string key = Context.GetHttpContext()?.Request.Query["key"].ToString()!;
            await Groups.AddGroupAsync(Context.ConnectionId, key);
        }
    }
}

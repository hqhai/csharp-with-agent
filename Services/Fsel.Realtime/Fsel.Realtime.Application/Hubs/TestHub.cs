// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Core.Services.IpApiServices;
    using Microsoft.AspNetCore.SignalR;

    public class TestHub : BaseHub
    {
        private readonly AuthContext _authContext;

        public TestHub(AuthContext authContext, IIpApiService ipApiService) : base(authContext, ipApiService)
        {
            _authContext = authContext;
        }

        public override async Task OnConnectedHubAsync()
        {
            string chatBotId = Context.GetHttpContext()?.Request.Query["TestId"].ToString()!;

            await Groups.AddGroupAsync(Context.ConnectionId, chatBotId);
        }
    }
}

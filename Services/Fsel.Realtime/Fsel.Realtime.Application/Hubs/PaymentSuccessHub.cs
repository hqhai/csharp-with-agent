// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Core.Services.IpApiServices;
    using Microsoft.AspNetCore.SignalR;

    public class PaymentSuccessHub : BaseHub
    {
        private readonly AuthContext _authContext;

        public PaymentSuccessHub(AuthContext authContext, IIpApiService ipApiService) : base(authContext, ipApiService)
        {
            _authContext = authContext;
        }

        public override async Task OnConnectedHubAsync()
        {
            string userId = Context.GetHttpContext()?.Request.Query["UserId"].ToString()!;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddGroupAsync(Context.ConnectionId, userId);
            }
        }
    }
}

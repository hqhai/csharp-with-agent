// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Core.Services.IpApiServices;
    using Microsoft.AspNetCore.Http;

    public class PaymentHub : BaseHub
    {
        private readonly AuthContext _authContext;

        public PaymentHub(AuthContext authContext, IIpApiService ipApiService, IHttpContextAccessor httpContextAccessor) : base(authContext, ipApiService, httpContextAccessor)
        {
            _authContext = authContext;
        }

        public override async Task OnConnectedHubAsync()
        {
            await Groups.AddGroupAsync(Context.ConnectionId, _authContext.CurrentUserId.ToString());
        }
    }
}

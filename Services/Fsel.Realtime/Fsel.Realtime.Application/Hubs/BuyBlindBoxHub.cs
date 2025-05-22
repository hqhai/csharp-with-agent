using Fsel.Core.Base;
using Fsel.Core.Extensions;
using Fsel.Core.Services.IpApiServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace Fsel.Realtime.Application.Hubs
{
    [Authorize]
    public class BuyBlindBoxHub : BaseHub
    {
        public BuyBlindBoxHub(AuthContext authContext, IIpApiService ipApiService, IHttpContextAccessor httpContextAccessor) : base(authContext, ipApiService, httpContextAccessor)
        {
        }

        public override async Task OnConnectedHubAsync()
        {
            string key = Context.GetHttpContext()?.Request.Query["userid"].ToString()!;
            await Groups.AddGroupAsync(Context.ConnectionId, key);
        }
    }
}

namespace Fsel.Realtime.Application.Hubs
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Core.Services.IpApiServices;
    using Fsel.Realtime.Application.Trackers;
    using Microsoft.AspNetCore.Http;

    public class TranscriptHub : BaseHub
    {
        private readonly AuthContext _authContext;

        public TranscriptHub(AuthContext authContext, IIpApiService ipApiService, IHttpContextAccessor httpContextAccessor) : base(authContext, ipApiService, httpContextAccessor)
        {
            _authContext = authContext;
        }

        public override async Task OnConnectedHubAsync()
        {
            await Groups.AddGroupAsync(Context.ConnectionId, _authContext.CurrentUserId.ToString());
            ConnectionTracker.Instance.RecordConnectionStart(Context.ConnectionId);
        }
        public override async Task OnDisconnectedHubAsync(Exception? exception)
        {
            await Groups.RemoveGroupAsync(Context.ConnectionId, _authContext.CurrentUserId.ToString());
        }
    }
}

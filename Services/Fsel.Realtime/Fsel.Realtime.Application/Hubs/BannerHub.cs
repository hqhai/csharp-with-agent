// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using System.Globalization;
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Core.Services.IpApiServices;
    using Fsel.Realtime.Application.Queues.Publishers;
    using Fsel.Realtime.Application.Trackers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SignalR;

    public class BannerHub : BaseHub
    {
        private readonly AuthContext _authContext;
        private readonly BannerPublisher _bannerPublisher;

        public BannerHub(AuthContext authContext, IIpApiService ipApiService, IHttpContextAccessor httpContextAccessor, BannerPublisher bannerPublisher) : base(authContext, ipApiService, httpContextAccessor)
        {
            _authContext = authContext;
            _bannerPublisher = bannerPublisher;
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

        public async Task GetBanner()
        {
            string date = (Context.GetHttpContext()?.Request.Query["Date"].ToString()!);
            if (string.IsNullOrEmpty(date))
            {
                return;
            }

            await _bannerPublisher.Publish(new Shared.Models.ShareModels.BannerMessageModel { Date = DateTime.Parse(date, CultureInfo.InvariantCulture), UserId = _authContext.CurrentUserId }, CancellationToken.None);
        }
    }
}

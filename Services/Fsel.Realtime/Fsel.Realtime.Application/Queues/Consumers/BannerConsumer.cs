// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Realtime.Application.Hubs;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SignalR;

    public class BannerConsumer : BaseConsumer<BannerStudentsQueueModel>
    {
        private readonly IHubContext<BannerHub> _bannerHub;

        public BannerConsumer(AuthContext authContext, IHttpContextAccessor httpContextAccessor, IHubContext<BannerHub> bannerHub) : base(authContext, httpContextAccessor)
        {
            _bannerHub = bannerHub;
        }

        public async override Task ConsumeQueue(BannerStudentsQueueModel? message)
        {
            if (message != null)
            {
                await _bannerHub.GetGroup(message.UserId.ToString()).SendAsync(RealtimeSettings.BannerHub.Methods.Banner, message);
            }
        }
    }
}

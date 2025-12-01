// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Http;

    public class TreasureZMatterWorker : BaseWorker
    {
        private readonly WeeklyNoticePublisher _weeklyNoticePublisher;

        public TreasureZMatterWorker(WeeklyNoticePublisher weeklyNoticePublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _weeklyNoticePublisher = weeklyNoticePublisher;
        }

        public override async Task RunAsync()
        {
            await _weeklyNoticePublisher.Publish(new WeeklyNoticeQueueModel { WeeklyNoticeType = EnumWeeklyNoticeType.TreasureZMatter }, CancellationToken.None);
        }
    }
}

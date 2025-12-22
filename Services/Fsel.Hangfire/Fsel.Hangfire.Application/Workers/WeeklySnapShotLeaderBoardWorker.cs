// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Http;

    public class WeeklySnapShotLeaderBoardWorker : BaseWorker
    {
        private readonly WeeklySnapShotLeaderBoardPublisher _weeklySnapShotPublisher;

        public WeeklySnapShotLeaderBoardWorker(WeeklySnapShotLeaderBoardPublisher weeklySnapShotPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _weeklySnapShotPublisher = weeklySnapShotPublisher;
        }

        public override async Task RunAsync()
        {
            await _weeklySnapShotPublisher.Publish(CancellationToken.None);
        }
    }
}

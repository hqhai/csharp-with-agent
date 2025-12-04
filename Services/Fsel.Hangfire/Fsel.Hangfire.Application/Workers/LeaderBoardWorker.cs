// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Http;

    public class LeaderBoardWorker : BaseWorker
    {
        private readonly LeaderBoardPublisher _leaderBoardPublisher;

        public LeaderBoardWorker(LeaderBoardPublisher leaderBoardPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _leaderBoardPublisher = leaderBoardPublisher;
        }

        public override async Task RunAsync()
        {
            await _leaderBoardPublisher.Publish(CancellationToken.None);
        }
    }
}

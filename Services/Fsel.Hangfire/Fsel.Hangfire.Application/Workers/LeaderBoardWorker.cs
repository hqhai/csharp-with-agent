// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class LeaderBoardWorker : IWorker
    {
        private readonly LeaderBoardPublisher _leaderBoardPublisher;

        public LeaderBoardWorker(LeaderBoardPublisher leaderBoardPublisher)
        {
            _leaderBoardPublisher = leaderBoardPublisher;
        }

        public async Task RunAsync()
        {
            await _leaderBoardPublisher.Publish(CancellationToken.None);
        }
    }
}

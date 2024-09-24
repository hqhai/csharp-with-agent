// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class WeeklySnapShotLeaderBoardWorker : IWorker
    {
        private readonly WeeklySnapShotLeaderBoardPublisher _weeklySnapShotPublisher;

        public WeeklySnapShotLeaderBoardWorker(WeeklySnapShotLeaderBoardPublisher weeklySnapShotPublisher)
        {
            _weeklySnapShotPublisher = weeklySnapShotPublisher;
        }

        public async Task RunAsync()
        {
            await _weeklySnapShotPublisher.Publish(CancellationToken.None);
        }
    }
}

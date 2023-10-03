// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class SyncStudentShieldEveryDayWorker : IWorker
    {
        private readonly SyncStudentShieldEveryDayPublisher _updateStudentsDailyStreakPublisher;

        public SyncStudentShieldEveryDayWorker(SyncStudentShieldEveryDayPublisher updateStudentsDailyStreakPublisher)
        {
            _updateStudentsDailyStreakPublisher = updateStudentsDailyStreakPublisher;
        }

        public async Task RunAsync()
        {
            await _updateStudentsDailyStreakPublisher.Publish(CancellationToken.None);
        }
    }
}

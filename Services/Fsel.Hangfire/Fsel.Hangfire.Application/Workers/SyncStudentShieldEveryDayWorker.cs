// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class SyncStudentShieldEveryDayWorker : IWorker
    {
        private readonly SyncStudentShieldEveryDayPublisher _syncStudentShieldEveryDayPublisher;

        public SyncStudentShieldEveryDayWorker(SyncStudentShieldEveryDayPublisher syncStudentShieldEveryDayPublisher)
        {
            _syncStudentShieldEveryDayPublisher = syncStudentShieldEveryDayPublisher;
        }

        public async Task RunAsync()
        {
            await _syncStudentShieldEveryDayPublisher.Publish(CancellationToken.None);
        }
    }
}

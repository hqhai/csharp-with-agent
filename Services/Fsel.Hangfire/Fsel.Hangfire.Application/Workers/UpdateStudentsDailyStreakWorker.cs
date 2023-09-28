// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class UpdateStudentsDailyStreakWorker : IWorker
    {
        private readonly UpdateStudentsDailyStreakPublisher _updateStudentsDailyStreakPublisher;

        public UpdateStudentsDailyStreakWorker(UpdateStudentsDailyStreakPublisher updateStudentsDailyStreakPublisher)
        {
            _updateStudentsDailyStreakPublisher = updateStudentsDailyStreakPublisher;
        }

        public async Task RunAsync()
        {
            await _updateStudentsDailyStreakPublisher.Publish(CancellationToken.None);
        }
    }
}

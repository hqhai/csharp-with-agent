// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class CheckUserDeletionWorker : IWorker
    {
        private readonly CheckUserDeletionPublisher _checkUserDeletionPublisher;

        public CheckUserDeletionWorker(CheckUserDeletionPublisher checkUserDeletionPublisher)
        {
            _checkUserDeletionPublisher = checkUserDeletionPublisher;
        }

        public async Task RunAsync()
        {
            await _checkUserDeletionPublisher.Publish(CancellationToken.None);
        }
    }
}

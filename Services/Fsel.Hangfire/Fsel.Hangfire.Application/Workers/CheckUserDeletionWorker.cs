// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class CheckUserDeletionWorker : IWorker
    {
        private readonly CheckUserDeletionPublisher _deleteAccountPublisher;

        public CheckUserDeletionWorker(CheckUserDeletionPublisher deleteAccountPublisher)
        {
            _deleteAccountPublisher = deleteAccountPublisher;
        }

        public async Task RunAsync()
        {
            await _deleteAccountPublisher.Publish(CancellationToken.None);
        }
    }
}

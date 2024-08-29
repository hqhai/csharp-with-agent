// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class DeleteAccountWorker : IWorker
    {
        private readonly DeleteAccountPublisher _deleteAccountPublisher;

        public DeleteAccountWorker(DeleteAccountPublisher deleteAccountPublisher)
        {
            _deleteAccountPublisher = deleteAccountPublisher;
        }

        public async Task RunAsync()
        {
            await _deleteAccountPublisher.Publish(CancellationToken.None);
        }
    }
}

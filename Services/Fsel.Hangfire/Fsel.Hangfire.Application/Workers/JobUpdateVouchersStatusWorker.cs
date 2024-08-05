// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class JobUpdateVouchersStatusWorker : IWorker
    {
        private readonly JobUpdateVouchersStatusPublisher _jobUpdateVouchersStatusPublisher;

        public JobUpdateVouchersStatusWorker(JobUpdateVouchersStatusPublisher jobUpdateVouchersStatusWorker)
        {
            _jobUpdateVouchersStatusPublisher = jobUpdateVouchersStatusWorker;
        }

        public async Task RunAsync()
        {
            await _jobUpdateVouchersStatusPublisher.Publish(CancellationToken.None);
        }
    }
}

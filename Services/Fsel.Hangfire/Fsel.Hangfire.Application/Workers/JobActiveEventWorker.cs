// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class JobActiveEventWorker : IWorker
    {
        private readonly JobActiveEventPublisher _jobActiveEventPublisher;

        public JobActiveEventWorker(JobActiveEventPublisher jobActiveEventPublisher)
        {
            _jobActiveEventPublisher = jobActiveEventPublisher;
        }

        public async Task RunAsync()
        {
            await _jobActiveEventPublisher.Publish(CancellationToken.None);
        }
    }
}

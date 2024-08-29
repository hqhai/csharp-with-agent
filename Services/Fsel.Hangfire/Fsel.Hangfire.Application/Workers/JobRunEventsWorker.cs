// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class JobRunEventsWorker : IWorker
    {
        private readonly JobRunEventsPublisher _jobRunEventPublisher;

        public JobRunEventsWorker(JobRunEventsPublisher jobRunEventPublisher)
        {
            _jobRunEventPublisher = jobRunEventPublisher;
        }

        public async Task RunAsync()
        {
            await _jobRunEventPublisher.Publish(CancellationToken.None);
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class JobStudentAggregateWorker : IWorker
    {
        private readonly JobStudentAggregatePublisher _jobStudentAggregatePublisher;

        public JobStudentAggregateWorker(JobStudentAggregatePublisher jobStudentAggregatePublisher)
        {
            _jobStudentAggregatePublisher = jobStudentAggregatePublisher;
        }

        public async Task RunAsync()
        {
            await _jobStudentAggregatePublisher.Publish(CancellationToken.None);
        }
    }
}

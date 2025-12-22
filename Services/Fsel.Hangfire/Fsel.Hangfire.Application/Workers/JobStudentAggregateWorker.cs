// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class JobStudentAggregateWorker : BaseWorker
    {
        private readonly JobStudentAggregatePublisher _jobStudentAggregatePublisher;

        public JobStudentAggregateWorker(JobStudentAggregatePublisher jobStudentAggregatePublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor, ILogger<PushNoticeWorker> logger) : base(authContext, httpContextAccessor)
        {
            _jobStudentAggregatePublisher = jobStudentAggregatePublisher;
        }

        public override async Task RunAsync()
        {
            await _jobStudentAggregatePublisher.Publish(CancellationToken.None);
        }
    }
}

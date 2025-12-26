// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Http;

    public class JobRunEventsWorker : BaseWorker
    {
        private readonly JobRunEventsPublisher _jobRunEventPublisher;

        public JobRunEventsWorker(JobRunEventsPublisher jobRunEventPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _jobRunEventPublisher = jobRunEventPublisher;
        }

        public override async Task RunAsync()
        {
            await _jobRunEventPublisher.Publish(CancellationToken.None);
        }
    }
}

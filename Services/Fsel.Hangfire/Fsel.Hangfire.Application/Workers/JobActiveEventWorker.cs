// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Http;

    public class JobActiveEventWorker : BaseWorker
    {
        private readonly JobActiveEventPublisher _jobActiveEventPublisher;

        public JobActiveEventWorker(JobActiveEventPublisher jobActiveEventPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _jobActiveEventPublisher = jobActiveEventPublisher;
        }

        public override async Task RunAsync()
        {
            await _jobActiveEventPublisher.Publish(CancellationToken.None);
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Http;

    public class SyncStudentShieldEveryDayWorker : BaseWorker
    {
        private readonly SyncStudentShieldEveryDayPublisher _syncStudentShieldEveryDayPublisher;

        public SyncStudentShieldEveryDayWorker(SyncStudentShieldEveryDayPublisher syncStudentShieldEveryDayPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _syncStudentShieldEveryDayPublisher = syncStudentShieldEveryDayPublisher;
        }

        public override async Task RunAsync()
        {
            await _syncStudentShieldEveryDayPublisher.Publish(CancellationToken.None);
        }
    }
}

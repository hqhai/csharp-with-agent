// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class NotifyWeeklyCourseGoalTargetWorker : BaseWorker
    {
        private readonly NotifyWeeklyCourseGoalTargetPublisher _notifyWeeklyCourseGoalTargetPublisher;

        public NotifyWeeklyCourseGoalTargetWorker(NotifyWeeklyCourseGoalTargetPublisher notifyWeeklyCourseGoalTargetPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor, ILogger<PushNoticeWorker> logger) : base(authContext, httpContextAccessor)
        {
            _notifyWeeklyCourseGoalTargetPublisher = notifyWeeklyCourseGoalTargetPublisher;
        }
        
        public override async Task RunAsync()
        {
            await _notifyWeeklyCourseGoalTargetPublisher.Publish(CancellationToken.None);
        }
    }
}

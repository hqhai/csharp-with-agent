// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class NotifyWeeklyCourseGoalTargetWorker : IWorker
    {
        private readonly NotifyWeeklyCourseGoalTargetPublisher _notifyWeeklyCourseGoalTargetPublisher;

        public NotifyWeeklyCourseGoalTargetWorker(NotifyWeeklyCourseGoalTargetPublisher notifyWeeklyCourseGoalTargetPublisher)
        {
            _notifyWeeklyCourseGoalTargetPublisher = notifyWeeklyCourseGoalTargetPublisher;
        }

        public async Task RunAsync()
        {
            await _notifyWeeklyCourseGoalTargetPublisher.Publish(CancellationToken.None);
        }
    }
}

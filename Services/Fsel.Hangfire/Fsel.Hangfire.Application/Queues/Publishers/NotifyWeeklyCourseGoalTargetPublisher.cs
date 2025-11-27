// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;

    public class NotifyWeeklyCourseGoalTargetPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public NotifyWeeklyCourseGoalTargetPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(CancellationToken cancellationToken)
        {
            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.NotifyWeeklyCourseGoalTarget, cancellationToken);
        }
    }
}

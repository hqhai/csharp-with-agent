// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Publishers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;

    public class NotifyWeeklyReportCourseTargetPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public NotifyWeeklyReportCourseTargetPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(CancellationToken cancellationToken)
        {
            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.NotifyWeeklyReportCourseTarget, cancellationToken);
        }
    }
}

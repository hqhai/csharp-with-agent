// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class NotifyWeeklyReportCourseTargetWorker : IWorker
    {
        private readonly NotifyWeeklyReportCourseTargetPublisher _notificationLessonCourseTargetPublisher;

        public NotifyWeeklyReportCourseTargetWorker(NotifyWeeklyReportCourseTargetPublisher notificationLessonCourseTargetPublisher)
        {
            _notificationLessonCourseTargetPublisher = notificationLessonCourseTargetPublisher;
        }

        public async Task RunAsync()
        {
            await _notificationLessonCourseTargetPublisher.Publish(CancellationToken.None);
        }
    }
}

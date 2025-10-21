// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class NotificationLessonCourseTargetWorker : IWorker
    {
        private readonly NotificationLessonCourseTargetPublisher _notificationLessonCourseTargetPublisher;

        public NotificationLessonCourseTargetWorker(NotificationLessonCourseTargetPublisher notificationLessonCourseTargetPublisher)
        {
            _notificationLessonCourseTargetPublisher = notificationLessonCourseTargetPublisher;
        }

        public async Task RunAsync()
        {
            await _notificationLessonCourseTargetPublisher.Publish(CancellationToken.None);
        }
    }
}

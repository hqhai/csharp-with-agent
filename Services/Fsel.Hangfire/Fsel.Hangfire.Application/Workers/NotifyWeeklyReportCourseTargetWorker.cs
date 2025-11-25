// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class NotifyWeeklyReportCourseTargetWorker : BaseWorker
    {
        private readonly NotifyWeeklyReportCourseTargetPublisher _notificationLessonCourseTargetPublisher;

        public NotifyWeeklyReportCourseTargetWorker(NotifyWeeklyReportCourseTargetPublisher notificationLessonCourseTargetPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor, ILogger<PushNoticeWorker> logger) : base(authContext, httpContextAccessor)
        {
            _notificationLessonCourseTargetPublisher = notificationLessonCourseTargetPublisher;
        }

        public override async Task RunAsync()
        {
            await _notificationLessonCourseTargetPublisher.Publish(CancellationToken.None);
        }
    }
}

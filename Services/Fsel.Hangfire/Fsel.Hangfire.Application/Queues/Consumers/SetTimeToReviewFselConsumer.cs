// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Hangfire.Application.Workers;
    using Fsel.Shared.Models.ShareModels;

    public class SetTimeToReviewFselConsumer : BaseConsumer<NotificationSendingQueueModel>
    {
        public SetTimeToReviewFselConsumer(AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
        }

        public override Task ConsumeQueue(NotificationSendingQueueModel? message)
        {
            if (message != null)
            {
                JobExtensions.SetScheduleJob<ReviewFselWorker, NotificationSendingQueueModel>(TimeSpan.FromDays(30), message);
            }
            return Task.CompletedTask;
        }
    }
}

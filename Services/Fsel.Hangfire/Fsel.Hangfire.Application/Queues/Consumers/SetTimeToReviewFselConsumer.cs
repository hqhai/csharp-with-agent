// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Consumers
{
    using Fsel.Core.Extensions;
    using Fsel.Hangfire.Application.Workers;
    using Fsel.Shared.Models.ShareModels;
    using MassTransit;

    public class SetTimeToReviewFselConsumer : IConsumer<NotificationSendingQueueModel>
    {
        public SetTimeToReviewFselConsumer()
        {
        }

        public Task Consume(ConsumeContext<NotificationSendingQueueModel> context)
        {
            if (context != null)
            {
                JobExtensions.SetScheduleJob<ReviewFselWorker, NotificationSendingQueueModel>(TimeSpan.FromDays(30), context.Message);
            }
            return Task.CompletedTask;
        }
    }
}

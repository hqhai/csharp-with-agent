// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Hangfire.Application.Workers;
    using Fsel.Shared.Models.ShareModels;
    using MassTransit;

    public class SetTimeToReviewFselConsumer : BaseConsumer<NotificationSendingQueueModel>
    {
        public SetTimeToReviewFselConsumer(AuthContext authContext) : base(authContext)
        {
        }

        public override Task ConsumeQueue(ConsumeContext<BaseQueueDataModel<NotificationSendingQueueModel>> context)
        {
            if (context != null)
            {
                JobExtensions.SetScheduleJob<ReviewFselWorker, NotificationSendingQueueModel>(TimeSpan.FromDays(30), context.Message?.Data);
            }
            return Task.CompletedTask;
        }
    }
}

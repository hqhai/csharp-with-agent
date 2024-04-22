// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Consumers
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Hangfire.Application.Workers;
    using Fsel.Shared.Constants;
    using MassTransit;

    public class SetTimeToClassForumApprovalConsumer : IConsumer<BaseQueueModel>
    {
        public SetTimeToClassForumApprovalConsumer()
        {
        }

        public Task Consume(ConsumeContext<BaseQueueModel> context)
        {
            if (context != null)
            {
                JobExtensions.SetScheduleJob<UpdateClassForumResultToExpiredTimeWorker, BaseQueueModel>(TimeSpan.FromHours(ValueSettings.DelayTwoHours), context.Message);
            }
            return Task.CompletedTask;
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Consumers
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Hangfire.Application.Workers;
    using Fsel.Shared.Constants;
    using MassTransit;

    public class SetTimeToClassForumApprovalConsumer : Core.Base.Interfaces.IBaseConsumer<BaseQueueModel>
    {
        public SetTimeToClassForumApprovalConsumer()
        {
        }

        public Task Consume(ConsumeContext<BaseQueueDataModel<BaseQueueModel>> context)
        {
            if (context != null)
            {
                JobExtensions.SetScheduleJob<UpdateClassForumResultToExpiredTimeWorker, BaseQueueModel>(TimeSpan.FromHours(ValueSettings.DelayTwoHours), context.Message.Data);
            }
            return Task.CompletedTask;
        }
    }
}

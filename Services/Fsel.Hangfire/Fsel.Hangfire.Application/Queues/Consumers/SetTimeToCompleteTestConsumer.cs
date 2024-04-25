// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Hangfire.Application.Workers;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using MassTransit;

    public class SetTimeToCompleteTestConsumer : BaseConsumer<SetTimeToCompleteTestModel>
    {
        public SetTimeToCompleteTestConsumer(AuthContext authContext) : base(authContext)
        {
        }

        public override Task ConsumeQueue(ConsumeContext<BaseQueueDataModel<SetTimeToCompleteTestModel>> context)
        {
            if (context != null)
            {
                JobExtensions.SetScheduleJob<CompleteTestWhenTimeOutWorker, SetTimeToCompleteTestModel>(TimeSpan.FromSeconds(context.Message.Data.ExecutionTime + ValueSettings.DelayWorkerSecond), context.Message.Data);
            }
            return Task.CompletedTask;
        }
    }
}

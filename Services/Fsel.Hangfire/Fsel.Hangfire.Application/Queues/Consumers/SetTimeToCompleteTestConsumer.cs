// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Consumers
{
    using Fsel.Core.Extensions;
    using Fsel.Hangfire.Application.Workers;
    using Fsel.Shared.Models.ShareModels;
    using MassTransit;

    public class SetTimeToCompleteTestConsumer : IConsumer<SetTimeToCompleteTestModel>
    {
        public SetTimeToCompleteTestConsumer()
        {
        }

        public Task Consume(ConsumeContext<SetTimeToCompleteTestModel> context)
        {
            if (context != null)
            {
                JobExtensions.SetScheduleJob<CompleteTestWhenTimeOutWorker, SetTimeToCompleteTestModel>(TimeSpan.FromSeconds(context.Message.ExecutionTime + 60), context.Message);
            }
            return Task.CompletedTask;
        }
    }
}

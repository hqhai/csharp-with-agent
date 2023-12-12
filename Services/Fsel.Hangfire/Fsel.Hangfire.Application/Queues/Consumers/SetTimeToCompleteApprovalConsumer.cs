// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Consumers
{
    using Fsel.Core.Extensions;
    using Fsel.Hangfire.Application.Workers;
    using Fsel.Shared.Models.ShareModels;
    using MassTransit;

    public class SetTimeToCompleteApprovalConsumer : IConsumer<SetTimeCompleteApprovalModel>
    {
        public SetTimeToCompleteApprovalConsumer()
        {
        }

        public Task Consume(ConsumeContext<SetTimeCompleteApprovalModel> context)
        {
            if (context != null)
            {
                double delayHour = (context.Message.ExpiredDate.Hour - DateTime.UtcNow.Hour);

                JobExtensions.SetScheduleJob<CompleteApprovalWhenTimeOutWorker, SetTimeCompleteApprovalModel>(TimeSpan.FromHours(delayHour), context.Message);

            }
            return Task.CompletedTask;
        }
    }
}

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
                JobExtensions.SetScheduleJob<CompleteApprovalWhenTimeOutWorker, SetTimeCompleteApprovalModel>(TimeSpan.FromSeconds((DateTime.UtcNow.Hour - context.Message.ExpiredDate.Hour) * 60), context.Message!);

            }
            return Task.CompletedTask;
        }
    }
}

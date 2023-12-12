// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Consumers
{
    using Fsel.Core.Extensions;
    using Fsel.Hangfire.Application.Workers;
    using Fsel.Shared.Models.ShareModels;
    using MassTransit;

    public class SetTimeToCompleteApprovalConsumer : IConsumer<SetTimeCompleteApprovalModel>
    {

        private const int Delay_Approve_Minutes = 1; // cộng thêm 1 phút trước khi chạy job để đảm bảo không có ai phê duyệt bài viết trước khi job chạy
        public SetTimeToCompleteApprovalConsumer()
        {
        }

        public Task Consume(ConsumeContext<SetTimeCompleteApprovalModel> context)
        {

            if (context != null)
            {
                JobExtensions.SetScheduleJob<CompleteApprovalWhenTimeOutWorker, SetTimeCompleteApprovalModel>(context.Message.StartDate.AddMinutes(Delay_Approve_Minutes), context.Message);
            }
            return Task.CompletedTask;
        }
    }
}

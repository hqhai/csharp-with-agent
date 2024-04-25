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

    public class SetTimeToCompleteApprovalConsumer : BaseConsumer<SetTimeCompleteApprovalModel>
    {
        public SetTimeToCompleteApprovalConsumer(AuthContext authContext) : base(authContext)
        {
        }

        public override Task ConsumeQueue(ConsumeContext<BaseQueueDataModel<SetTimeCompleteApprovalModel>> context)
        {
            if (context != null)
            {
                // cộng thêm 1 phút trước khi chạy job để đảm bảo không có ai phê duyệt bài viết trước khi job chạy
                JobExtensions.SetScheduleJob<CompleteApprovalWhenTimeOutWorker, SetTimeCompleteApprovalModel>(context.Message.Data.StartDate.AddMinutes(ValueSettings.DelayOneMinute), context.Message.Data);
            }
            return Task.CompletedTask;
        }
    }
}

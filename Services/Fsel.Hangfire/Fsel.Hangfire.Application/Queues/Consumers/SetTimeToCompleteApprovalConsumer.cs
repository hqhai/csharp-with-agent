// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Hangfire.Application.Workers;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class SetTimeToCompleteApprovalConsumer : BaseConsumer<SetTimeCompleteApprovalModel>
    {
        public SetTimeToCompleteApprovalConsumer(AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
        }

        public override Task ConsumeQueue(SetTimeCompleteApprovalModel? message)
        {
            if (message != null)
            {
                // cộng thêm 1 phút trước khi chạy job để đảm bảo không có ai phê duyệt bài viết trước khi job chạy
                JobExtensions.SetScheduleJob<CompleteApprovalWhenTimeOutWorker, SetTimeCompleteApprovalModel>(message.StartDate.AddMinutes(ValueSettings.DelayOneMinute), message);
            }
            return Task.CompletedTask;
        }
    }
}

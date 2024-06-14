// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Hangfire.Application.Workers;
    using Fsel.Shared.Models.ShareModels;

    public class SetTimeToRetryMockTestConsumer : BaseConsumer<SetTimeRetryMockTestModel>
    {
        public SetTimeToRetryMockTestConsumer(AuthContext authContext) : base(authContext)
        {
        }

        public override Task ConsumeQueue(SetTimeRetryMockTestModel? message)
        {
            if (message != null)
            {
                // cộng thêm 1 phút trước khi chạy job để đảm bảo không có ai phê duyệt bài viết trước khi job chạy
                JobExtensions.SetScheduleJob<RetryMockTestWhenNotReturnScoreWorker, SetTimeRetryMockTestModel>(message.StartDate.AddSeconds(30), message);
            }
            return Task.CompletedTask;
        }
    }
}

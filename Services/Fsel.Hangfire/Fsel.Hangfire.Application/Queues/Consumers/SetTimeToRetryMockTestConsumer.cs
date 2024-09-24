// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Hangfire.Application.Workers;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class SetTimeToRetryMockTestConsumer : BaseConsumer<SetTimeRetryMockTestModel>
    {
        public SetTimeToRetryMockTestConsumer(AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
        }

        public override Task ConsumeQueue(SetTimeRetryMockTestModel? message)
        {
            if (message != null)
            {
                JobExtensions.SetScheduleJob<RetryMockTestWhenNotReturnScoreWorker, SetTimeRetryMockTestModel>(message.StartDate.AddMinutes(ValueSettings.DelayThreeMinute), message);
            }
            return Task.CompletedTask;
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Hangfire.Application.Workers;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class SetTimeToCompleteTestConsumer : BaseConsumer<SetTimeToCompleteTestModel>
    {
        public SetTimeToCompleteTestConsumer(AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
        }

        public override Task ConsumeQueue(SetTimeToCompleteTestModel? message)
        {
            if (message != null)
            {
                JobExtensions.SetScheduleJob<CompleteTestWhenTimeOutWorker, SetTimeToCompleteTestModel>(TimeSpan.FromSeconds(message.ExecutionTime + ValueSettings.DelayWorkerSecond), message);
            }
            return Task.CompletedTask;
        }
    }
}

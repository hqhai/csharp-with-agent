// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Consumers
{
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Hangfire.Application.Workers;
    using Fsel.Shared.Constants;
    using Microsoft.Extensions.Hosting;

    public class SetTimeToClassForumApprovalConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IHostEnvironment _environment;

        public SetTimeToClassForumApprovalConsumer(IHostEnvironment environment, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _environment = environment;
        }

        public override Task ConsumeQueue(BaseQueueModel? message)
        {
            if (message != null)
            {
                if (_environment.IsDevelopment() || _environment.IsEnvironment(Settings.Environments.Testing) || _environment.IsStaging())
                {
                    JobExtensions.SetScheduleJob<UpdateClassForumResultToExpiredTimeWorker, BaseQueueModel>(TimeSpan.FromMinutes(ValueSettings.DelayTenMinutes), message);
                }
                else if (_environment.IsProduction())
                {
                    JobExtensions.SetScheduleJob<UpdateClassForumResultToExpiredTimeWorker, BaseQueueModel>(TimeSpan.FromHours(ValueSettings.DelayTwoHours), message);
                }
            }
            return Task.CompletedTask;
        }
    }
}

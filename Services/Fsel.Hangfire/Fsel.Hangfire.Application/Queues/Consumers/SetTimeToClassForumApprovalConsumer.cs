// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Queues.Consumers
{
    using Fsel.Common.Constants;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Hangfire.Application.Workers;
    using Fsel.Shared.Constants;
    using MassTransit;
    using Microsoft.Extensions.Hosting;

    public class SetTimeToClassForumApprovalConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IHostEnvironment _environment;

        public SetTimeToClassForumApprovalConsumer(IHostEnvironment environment, AuthContext authContext) : base(authContext)
        {
            _environment = environment;
        }

        public override Task ConsumeQueue(ConsumeContext<BaseQueueDataModel<BaseQueueModel>> context)
        {
            if (context != null)
            {
                if (_environment.IsDevelopment() || _environment.IsEnvironment(Settings.Environments.Testing) || _environment.IsStaging())
                {
                    JobExtensions.SetScheduleJob<UpdateClassForumResultToExpiredTimeWorker, BaseQueueModel>(TimeSpan.FromMinutes(ValueSettings.DelayTenMinutes), context.Message.Data);
                }
                else if (_environment.IsProduction())
                {
                    JobExtensions.SetScheduleJob<UpdateClassForumResultToExpiredTimeWorker, BaseQueueModel>(TimeSpan.FromHours(ValueSettings.DelayTwoHours), context.Message.Data);
                }
            }
            return Task.CompletedTask;
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Http;

    public class CompleteTestWhenTimeOutWorker : BaseWorker<SetTimeToCompleteTestModel>
    {
        private readonly CompleteTestWhenTimeOutPublisher _completeTestWhenTimeOutPublisher;

        public CompleteTestWhenTimeOutWorker(CompleteTestWhenTimeOutPublisher completeTestWhenTimeOutPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _completeTestWhenTimeOutPublisher = completeTestWhenTimeOutPublisher;
        }

        public override async Task RunAsync(SetTimeToCompleteTestModel? data = null)
        {
            if (data != null)
            {
                await _completeTestWhenTimeOutPublisher.Publish(data, CancellationToken.None);
            }
        }
    }
}

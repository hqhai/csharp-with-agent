// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Http;

    public class CompleteApprovalWhenTimeOutWorker : BaseWorker<SetTimeCompleteApprovalModel>
    {
        private readonly CompleteApprovalPosWhenTimeOutPublisher _completeApprovalPosWhenTimeOutPublisher;

        public CompleteApprovalWhenTimeOutWorker(CompleteApprovalPosWhenTimeOutPublisher completeApprovalPosWhenTimeOutPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _completeApprovalPosWhenTimeOutPublisher = completeApprovalPosWhenTimeOutPublisher;
        }

        public override async Task RunAsync(SetTimeCompleteApprovalModel? data)
        {
            if (data != null)
            {
                await _completeApprovalPosWhenTimeOutPublisher.Publish(data, CancellationToken.None);
            }
        }
    }
}

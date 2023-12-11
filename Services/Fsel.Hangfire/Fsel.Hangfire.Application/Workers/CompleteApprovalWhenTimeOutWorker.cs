// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;

    public class CompleteApprovalWhenTimeOutWorker : IWorker<SetTimeCompleteApprovalModel>
    {
        private readonly CompleteApprovalPosWhenTimeOutPublisher _completeApprovalPosWhenTimeOutPublisher;

        public CompleteApprovalWhenTimeOutWorker(CompleteApprovalPosWhenTimeOutPublisher completeApprovalPosWhenTimeOutPublisher)
        {
            _completeApprovalPosWhenTimeOutPublisher = completeApprovalPosWhenTimeOutPublisher;
        }

        public async Task RunAsync(SetTimeCompleteApprovalModel? data = null)
        {
            if (data != null)
            {
                await _completeApprovalPosWhenTimeOutPublisher.Publish(data, CancellationToken.None);
            }
        }
    }
}

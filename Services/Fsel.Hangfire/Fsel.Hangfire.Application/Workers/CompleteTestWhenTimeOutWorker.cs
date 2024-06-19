// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;

    public class CompleteTestWhenTimeOutWorker : IWorker<SetTimeToCompleteTestModel>
    {
        private readonly CompleteTestWhenTimeOutPublisher _completeTestWhenTimeOutPublisher;

        public CompleteTestWhenTimeOutWorker(CompleteTestWhenTimeOutPublisher completeTestWhenTimeOutPublisher)
        {
            _completeTestWhenTimeOutPublisher = completeTestWhenTimeOutPublisher;
        }

        public async Task RunAsync(SetTimeToCompleteTestModel? data = null)
        {
            if (data != null)
            {
                await _completeTestWhenTimeOutPublisher.Publish(data, CancellationToken.None);
            }
        }
    }
}

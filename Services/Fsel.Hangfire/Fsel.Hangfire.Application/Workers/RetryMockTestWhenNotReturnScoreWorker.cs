// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;

    public class RetryMockTestWhenNotReturnScoreWorker : IWorker<SetTimeRetryMockTestModel>
    {
        private readonly RetryMockTestPublisher _retryMockTestPublisher;

        public RetryMockTestWhenNotReturnScoreWorker(RetryMockTestPublisher retryMockTestPublisher)
        {
            _retryMockTestPublisher = retryMockTestPublisher;
        }

        public async Task RunAsync(SetTimeRetryMockTestModel? data)
        {
            if (data != null)
            {
                await _retryMockTestPublisher.Publish(data, CancellationToken.None);
            }
        }
    }
}

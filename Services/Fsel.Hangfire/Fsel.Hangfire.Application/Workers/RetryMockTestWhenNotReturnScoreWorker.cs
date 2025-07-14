// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Http;

    public class RetryMockTestWhenNotReturnScoreWorker : BaseWorker<SetTimeRetryMockTestModel>
    {
        private readonly RetryMockTestPublisher _retryMockTestPublisher;

        public RetryMockTestWhenNotReturnScoreWorker(RetryMockTestPublisher retryMockTestPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _retryMockTestPublisher = retryMockTestPublisher;
        }

        public override async Task RunAsync(SetTimeRetryMockTestModel? data)
        {
            if (data != null)
            {
                await _retryMockTestPublisher.Publish(data, CancellationToken.None);
            }
        }
    }
}

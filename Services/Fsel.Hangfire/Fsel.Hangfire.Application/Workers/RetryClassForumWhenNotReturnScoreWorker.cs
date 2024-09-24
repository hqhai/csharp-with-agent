// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;

    public class RetryClassForumWhenNotReturnScoreWorker : IWorker<SetTimeRetryClassForumModel>
    {
        private readonly RetryClassForumPublisher _retryClassForumPublisher;

        public RetryClassForumWhenNotReturnScoreWorker(RetryClassForumPublisher retryClassForumPublisher)
        {
            _retryClassForumPublisher = retryClassForumPublisher;
        }

        public async Task RunAsync(SetTimeRetryClassForumModel? data)
        {
            if (data != null)
            {
                await _retryClassForumPublisher.Publish(data, CancellationToken.None);
            }
        }
    }
}

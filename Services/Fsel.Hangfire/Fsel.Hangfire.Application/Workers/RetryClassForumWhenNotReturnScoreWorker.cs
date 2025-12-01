// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Http;

    public class RetryClassForumWhenNotReturnScoreWorker : BaseWorker<SetTimeRetryClassForumModel>
    {
        private readonly RetryClassForumPublisher _retryClassForumPublisher;

        public RetryClassForumWhenNotReturnScoreWorker(RetryClassForumPublisher retryClassForumPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _retryClassForumPublisher = retryClassForumPublisher;
        }

        public override async Task RunAsync(SetTimeRetryClassForumModel? data)
        {
            if (data != null)
            {
                await _retryClassForumPublisher.Publish(data, CancellationToken.None);
            }
        }
    }
}

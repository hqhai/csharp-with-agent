// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Http;

    public class ReviewFselWorker : BaseWorker<NotificationSendingQueueModel>
    {
        private readonly ReviewFselPublisher _reviewFselPublisher;

        public ReviewFselWorker(ReviewFselPublisher reviewFselPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _reviewFselPublisher = reviewFselPublisher;
        }

        public override async Task RunAsync(NotificationSendingQueueModel? data = null)
        {
            await _reviewFselPublisher.Publish(data, CancellationToken.None);
        }
    }
}

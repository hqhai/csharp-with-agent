// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;

    public class ReviewFselWorker : IWorker<NotificationSendingQueueModel>
    {
        private readonly ReviewFselPublisher _reviewFselPublisher;

        public ReviewFselWorker(ReviewFselPublisher reviewFselPublisher)
        {
            _reviewFselPublisher = reviewFselPublisher;
        }

        public async Task RunAsync(NotificationSendingQueueModel? data = null)
        {
            await _reviewFselPublisher.Publish(data, CancellationToken.None);
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class NoticeAccessTimeWorker : IWorker
    {
        private readonly NoticeAccessTimePublisher _noticeAccessTimePublisher;

        public NoticeAccessTimeWorker(NoticeAccessTimePublisher noticeAccessTimePublisher)
        {
            _noticeAccessTimePublisher = noticeAccessTimePublisher;
        }

        public async Task RunAsync()
        {
            await _noticeAccessTimePublisher.Publish(CancellationToken.None);
        }
    }
}

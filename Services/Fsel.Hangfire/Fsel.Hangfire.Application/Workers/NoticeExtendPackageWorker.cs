// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class NoticeExtendPackageWorker : IWorker
    {
        private readonly NoticeAccessTimePublisher _noticeAccessTimePublisher;

        public NoticeExtendPackageWorker(NoticeAccessTimePublisher noticeAccessTimePublisher)
        {
            _noticeAccessTimePublisher = noticeAccessTimePublisher;
        }

        public async Task RunAsync()
        {
            await _noticeAccessTimePublisher.Publish(CancellationToken.None);
        }
    }
}

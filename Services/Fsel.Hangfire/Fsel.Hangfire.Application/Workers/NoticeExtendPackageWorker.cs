// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class NoticeExtendPackageWorker : IWorker
    {
        private readonly NoticeExtendPackagePublisher _noticeExtendPackagePublisher;

        public NoticeExtendPackageWorker(NoticeExtendPackagePublisher noticeExtendPackagePublisher)
        {
            _noticeExtendPackagePublisher = noticeExtendPackagePublisher;
        }

        public async Task RunAsync()
        {
            await _noticeExtendPackagePublisher.Publish(CancellationToken.None);
        }
    }
}

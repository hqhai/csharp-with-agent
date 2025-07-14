// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Http;

    public class NoticeExtendPackageWorker : BaseWorker
    {
        private readonly NoticeExtendPackagePublisher _noticeExtendPackagePublisher;

        public NoticeExtendPackageWorker(NoticeExtendPackagePublisher noticeExtendPackagePublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _noticeExtendPackagePublisher = noticeExtendPackagePublisher;
        }

        public override async Task RunAsync()
        {
            await _noticeExtendPackagePublisher.Publish(CancellationToken.None);
        }
    }
}

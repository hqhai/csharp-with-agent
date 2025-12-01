// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Http;

    public class NoticeAccessTimeWorker : BaseWorker
    {
        private readonly NoticeAccessTimePublisher _noticeAccessTimePublisher;

        public NoticeAccessTimeWorker(NoticeAccessTimePublisher noticeAccessTimePublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _noticeAccessTimePublisher = noticeAccessTimePublisher;
        }

        public override async Task RunAsync()
        {
            await _noticeAccessTimePublisher.Publish(CancellationToken.None);
        }
    }
}

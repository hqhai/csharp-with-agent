// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Http;

    public class UpdateClassForumResultToExpiredTimeWorker : BaseWorker<BaseQueueModel>
    {
        private readonly UpdateClassForumResultToExpiredTimePublisher _updateClassForumResultToExpiredTimePublisher;

        public UpdateClassForumResultToExpiredTimeWorker(UpdateClassForumResultToExpiredTimePublisher updateClassForumResultToExpiredTimePublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _updateClassForumResultToExpiredTimePublisher = updateClassForumResultToExpiredTimePublisher;
        }

        public override async Task RunAsync(BaseQueueModel? data = null)
        {
            if (data != null)
            {
                await _updateClassForumResultToExpiredTimePublisher.Publish(data, CancellationToken.None);
            }
        }
    }
}

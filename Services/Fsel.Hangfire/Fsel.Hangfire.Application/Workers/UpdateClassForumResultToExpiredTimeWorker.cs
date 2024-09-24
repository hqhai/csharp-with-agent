// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class UpdateClassForumResultToExpiredTimeWorker : IWorker<BaseQueueModel>
    {
        private readonly UpdateClassForumResultToExpiredTimePublisher _updateClassForumResultToExpiredTimePublisher;

        public UpdateClassForumResultToExpiredTimeWorker(UpdateClassForumResultToExpiredTimePublisher updateClassForumResultToExpiredTimePublisher)
        {
            _updateClassForumResultToExpiredTimePublisher = updateClassForumResultToExpiredTimePublisher;
        }

        public async Task RunAsync(BaseQueueModel? data = null)
        {
            if (data != null)
            {
                await _updateClassForumResultToExpiredTimePublisher.Publish(data, CancellationToken.None);
            }
        }
    }
}

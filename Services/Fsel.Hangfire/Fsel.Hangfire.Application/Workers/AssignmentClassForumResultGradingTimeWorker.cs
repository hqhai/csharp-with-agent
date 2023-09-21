// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class AssignmentClassForumResultGradingTimeWorker : IWorker
    {
        private readonly UpdateClassForumResultGradingTimePublisher _updateClassForumResultPublisher;
        public AssignmentClassForumResultGradingTimeWorker(UpdateClassForumResultGradingTimePublisher updateClassForumResultPublisher)
        {
            _updateClassForumResultPublisher = updateClassForumResultPublisher;
        }

        public async Task RunAsync()
        {
            await _updateClassForumResultPublisher.Publish(CancellationToken.None);
        }
    }
}

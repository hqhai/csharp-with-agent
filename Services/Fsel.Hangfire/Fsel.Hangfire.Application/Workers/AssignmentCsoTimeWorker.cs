// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class AssignmentCsoTimeWorker : IWorker
    {
        private readonly UpdateClassForumResultPublisher _updateClassForumResultPublisher;
        public AssignmentCsoTimeWorker(UpdateClassForumResultPublisher updateClassForumResultPublisher)
        {
            _updateClassForumResultPublisher = updateClassForumResultPublisher;
        }
        public async Task RunAsync<T>(T? data = null) where T : class
        {
            await _updateClassForumResultPublisher.Publish(CancellationToken.None);
        }

        public async Task RunAsync()
        {
            await _updateClassForumResultPublisher.Publish(CancellationToken.None);
        }
    }
}

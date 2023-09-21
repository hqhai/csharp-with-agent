// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class AssignmentClassForumResultCheckTimeWorker : IWorker
    {
        private readonly UpdateOcCheckTimePublisher _updateOcCheckTimePublisher;

        public AssignmentClassForumResultCheckTimeWorker(UpdateOcCheckTimePublisher updateOcCheckTimePublisher)
        {
            _updateOcCheckTimePublisher = updateOcCheckTimePublisher;
        }

        public async Task RunAsync()
        {
            await _updateOcCheckTimePublisher.Publish(CancellationToken.None);
        }
    }
}

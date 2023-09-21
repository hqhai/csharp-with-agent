// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class UpdateTeacherGradingInClassForumAndMockTestWorker : IWorker
    {
        private readonly UpdateOcCheckInClassForumResultPublisher _updateClassForumResultPublisher;
        public UpdateTeacherGradingInClassForumAndMockTestWorker(UpdateOcCheckInClassForumResultPublisher updateClassForumResultPublisher)
        {
            _updateClassForumResultPublisher = updateClassForumResultPublisher;
        }

        public async Task RunAsync()
        {
            await _updateClassForumResultPublisher.Publish(CancellationToken.None);
        }
    }
}

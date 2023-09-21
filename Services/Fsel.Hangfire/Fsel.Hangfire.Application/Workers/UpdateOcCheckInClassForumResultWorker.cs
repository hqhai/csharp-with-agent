// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class UpdateOcCheckInClassForumResultWorker : IWorker
    {
        private readonly UpdateTeacherGradingInClassForumAndMockTestPublisher _updateOcCheckTimePublisher;

        public UpdateOcCheckInClassForumResultWorker(UpdateTeacherGradingInClassForumAndMockTestPublisher updateOcCheckTimePublisher)
        {
            _updateOcCheckTimePublisher = updateOcCheckTimePublisher;
        }

        public async Task RunAsync()
        {
            await _updateOcCheckTimePublisher.Publish(CancellationToken.None);
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class EndTrialRegistrationWorker : IWorker
    {
        private readonly UpdateStatusTrialStudentPublisher _updateStatusTrialStudentPublisher;

        public EndTrialRegistrationWorker(UpdateStatusTrialStudentPublisher updateStatusTrialStudentPublisher)
        {
            _updateStatusTrialStudentPublisher = updateStatusTrialStudentPublisher;
        }

        public async Task RunAsync()
        {
            await _updateStatusTrialStudentPublisher.Publish(CancellationToken.None);
        }
    }
}

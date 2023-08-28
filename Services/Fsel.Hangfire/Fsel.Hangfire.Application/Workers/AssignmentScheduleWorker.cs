// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class AssignmentScheduleWorker : IWorker
    {
        private readonly UpdateClassLiveAssignmentPublisher _updateClassLiveAssignmentPublisher;

        public AssignmentScheduleWorker(UpdateClassLiveAssignmentPublisher updateClassLiveAssignmentPublisher)
        {
            _updateClassLiveAssignmentPublisher = updateClassLiveAssignmentPublisher;
        }

        public async Task RunAsync<T>(T? data = null) where T : class
        {
            await _updateClassLiveAssignmentPublisher.Publish(CancellationToken.None);
        }

        public async Task RunAsync()
        {
            await _updateClassLiveAssignmentPublisher.Publish(CancellationToken.None);
        }
    }
}

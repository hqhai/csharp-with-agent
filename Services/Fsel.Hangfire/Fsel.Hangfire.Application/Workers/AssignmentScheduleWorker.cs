// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Http;

    public class AssignmentScheduleWorker : BaseWorker
    {
        private readonly UpdateClassLiveAssignmentPublisher _updateClassLiveAssignmentPublisher;

        public AssignmentScheduleWorker(UpdateClassLiveAssignmentPublisher updateClassLiveAssignmentPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _updateClassLiveAssignmentPublisher = updateClassLiveAssignmentPublisher;
        }

        public override async Task RunAsync()
        {
            await _updateClassLiveAssignmentPublisher.Publish(CancellationToken.None);
        }
    }
}

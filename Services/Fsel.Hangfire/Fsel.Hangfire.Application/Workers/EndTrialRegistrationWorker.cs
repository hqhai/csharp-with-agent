// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Http;

    public class EndTrialRegistrationWorker : BaseWorker
    {
        private readonly UpdateStatusTrialStudentPublisher _updateStatusTrialStudentPublisher;

        public EndTrialRegistrationWorker(UpdateStatusTrialStudentPublisher updateStatusTrialStudentPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _updateStatusTrialStudentPublisher = updateStatusTrialStudentPublisher;
        }

        public override async Task RunAsync()
        {
            await _updateStatusTrialStudentPublisher.Publish(CancellationToken.None);
        }
    }
}

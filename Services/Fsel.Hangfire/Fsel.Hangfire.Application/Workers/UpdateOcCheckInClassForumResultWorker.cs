// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Http;

    public class UpdateOcCheckInClassForumResultWorker : BaseWorker
    {
        private readonly UpdateTeacherGradingInClassForumAndMockTestPublisher _updateOcCheckTimePublisher;

        public UpdateOcCheckInClassForumResultWorker(UpdateTeacherGradingInClassForumAndMockTestPublisher updateOcCheckTimePublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _updateOcCheckTimePublisher = updateOcCheckTimePublisher;
        }

        public override async Task RunAsync()
        {
            await _updateOcCheckTimePublisher.Publish(CancellationToken.None);
        }
    }
}

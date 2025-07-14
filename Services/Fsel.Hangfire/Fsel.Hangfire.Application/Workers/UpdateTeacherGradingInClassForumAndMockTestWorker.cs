// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Http;

    public class UpdateTeacherGradingInClassForumAndMockTestWorker : BaseWorker
    {
        private readonly UpdateOcCheckInClassForumResultPublisher _updateClassForumResultPublisher;
        public UpdateTeacherGradingInClassForumAndMockTestWorker(UpdateOcCheckInClassForumResultPublisher updateClassForumResultPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _updateClassForumResultPublisher = updateClassForumResultPublisher;
        }

        public override async Task RunAsync()
        {
            await _updateClassForumResultPublisher.Publish(CancellationToken.None);
        }
    }
}

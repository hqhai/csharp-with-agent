// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Http;

    public class CheckUserDeletionWorker : BaseWorker
    {
        private readonly CheckUserDeletionPublisher _checkUserDeletionPublisher;

        public CheckUserDeletionWorker(CheckUserDeletionPublisher checkUserDeletionPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _checkUserDeletionPublisher = checkUserDeletionPublisher;
        }

        public override async Task RunAsync()
        {
            await _checkUserDeletionPublisher.Publish(CancellationToken.None);
        }
    }
}

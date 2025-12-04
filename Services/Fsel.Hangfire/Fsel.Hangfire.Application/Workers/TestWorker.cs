// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Microsoft.AspNetCore.Http;

namespace Fsel.Hangfire.Application.Workers
{
    public class TestWorker : BaseWorker
    {
        public TestWorker(AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
        }

        public override Task RunAsync()
        {
            return Task.CompletedTask;
        }
    }
}

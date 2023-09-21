// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;

namespace Fsel.Hangfire.Application.Workers
{
    public class TestWorker : IWorker
    {
        public Task RunAsync()
        {
            return Task.CompletedTask;
        }
    }
}

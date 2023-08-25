// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;

namespace Fsel.Hangfire.Application.Workers
{
    public class AssignmentScheduleWorker : IWorker
    {
        public Task RunAsync<T>(T? data = null) where T : class
        {
            return Task.CompletedTask;
        }

        public Task RunAsync()
        {
            return Task.CompletedTask;
        }
    }
}

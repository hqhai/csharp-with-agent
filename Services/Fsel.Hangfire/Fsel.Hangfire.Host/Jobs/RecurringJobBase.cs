// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Host.Jobs
{
    using Fsel.Core.Extensions;
    using Fsel.Hangfire.Application.Workers;
    using Fsel.Shared.Constants;
    using global::Hangfire;

    public static class RecurringJobBase
    {
        public static void Setup()
        {
            JobExtensions.SetEnqueueJob<TestWorker>();
            JobExtensions.SetRecurringJob<AssignmentScheduleWorker>(WorkerSettings.JobName.AssignmentScheduleJob, Cron.Hourly);
            //JobExtensions.SetRecurringJob<TestWorker>(WorkerSettings.JobName.TestWorkerJob, Cron.Daily);
        }
    }
}

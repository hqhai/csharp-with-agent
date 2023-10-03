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
            JobExtensions.SetRecurringJob<AssignmentScheduleWorker>(WorkerSettings.JobName.AssignmentScheduleJob, Cron.MinuteInterval(1));
            JobExtensions.SetRecurringJob<UpdateTeacherGradingInClassForumAndMockTestWorker>(WorkerSettings.JobName.UpdateOcCheckInClassForumResultJob, Cron.MinuteInterval(3));
            JobExtensions.SetRecurringJob<SyncStudentShieldEveryDayWorker>(WorkerSettings.JobName.SyncStudentShieldEveryDayJob, Cron.MinuteInterval(1) /*Cron.Daily()*/);
            JobExtensions.SetRecurringJob<UpdateOcCheckInClassForumResultWorker>(WorkerSettings.JobName.UpdateTeacherGradingInClassForumAndMockTestJob, Cron.MinuteInterval(3));
            //JobExtensions.SetRecurringJob<TestWorker>(WorkerSettings.JobName.TestWorkerJob, Cron.Daily);
        }
    }
}

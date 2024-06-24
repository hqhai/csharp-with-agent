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
            //JobExtensions.SetRecurringJob<AssignmentScheduleWorker>(WorkerSettings.JobName.AssignmentScheduleJob, Cron.MinuteInterval(15));
            JobExtensions.SetRecurringJob<EndTrialRegistrationWorker>(WorkerSettings.JobName.EndTrialRegistrationJob, Cron.Daily());
            JobExtensions.SetRecurringJob<UpdateTeacherGradingInClassForumAndMockTestWorker>(WorkerSettings.JobName.UpdateOcCheckInClassForumResultJob, Cron.MinuteInterval(15));
            JobExtensions.SetRecurringJob<SyncStudentShieldEveryDayWorker>(WorkerSettings.JobName.SyncStudentShieldEveryDayJob, Cron.Daily());
            JobExtensions.SetRecurringJob<UpdateOcCheckInClassForumResultWorker>(WorkerSettings.JobName.UpdateTeacherGradingInClassForumAndMockTestJob, Cron.MinuteInterval(15));
            JobExtensions.SetRecurringJob<EndTrialRegistrationWorker>(WorkerSettings.JobName.EndTrialRegistrationJob, Cron.Daily());
            JobExtensions.SetRecurringJob<LeaderBoardWorker>(WorkerSettings.JobName.LeaderBoardJob, Cron.HourInterval(1));
            JobExtensions.SetRecurringJob<NoticeAccessTimeWorker>(WorkerSettings.JobName.NoticeAccessTime, Cron.Daily());
            JobExtensions.SetRecurringJob<WeeklyReportWorker>(WorkerSettings.JobName.WeeklyReport, Cron.Daily(12, 35));
            //JobExtensions.SetRecurringJob<TestWorker>(WorkerSettings.JobName.TestWorkerJob, Cron.Daily);
        }
    }
}

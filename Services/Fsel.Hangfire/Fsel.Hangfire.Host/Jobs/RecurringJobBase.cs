// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Host.Jobs
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Extensions;
    using Fsel.Hangfire.Application.Classes;
    using Fsel.Hangfire.Application.Workers;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using global::Hangfire;

    public static class RecurringJobBase
    {
        public static void Setup()
        {
            //JobExtensions.SetEnqueueJob<TestWorker>();
            //JobExtensions.SetRecurringJob<AssignmentScheduleWorker>(WorkerSettings.JobName.AssignmentScheduleJob, Cron.MinuteInterval(15), allTenants: true);
            JobExtensions.SetRecurringJob<EndTrialRegistrationWorker>(WorkerSettings.JobName.EndTrialRegistrationJob, Cron.Daily(), allTenants: true);
            //JobExtensions.SetRecurringJob<UpdateTeacherGradingInClassForumAndMockTestWorker>(WorkerSettings.JobName.UpdateOcCheckInClassForumResultJob, Cron.MinuteInterval(15), allTenants: true);
            JobExtensions.SetRecurringJob<SyncStudentShieldEveryDayWorker>(WorkerSettings.JobName.SyncStudentShieldEveryDayJob, Cron.Daily(), allTenants: true);
            //JobExtensions.SetRecurringJob<UpdateOcCheckInClassForumResultWorker>(WorkerSettings.JobName.UpdateTeacherGradingInClassForumAndMockTestJob, Cron.MinuteInterval(15), allTenants: true);
            JobExtensions.SetRecurringJob<LeaderBoardWorker>(WorkerSettings.JobName.LeaderBoardJob, Cron.HourInterval(1), allTenants: true);
            JobExtensions.SetRecurringJob<NoticeAccessTimeWorker>(WorkerSettings.JobName.NoticeAccessTime, Cron.HourInterval(1), allTenants: true);
            JobExtensions.SetRecurringJob<NoticeExtendPackageWorker>(WorkerSettings.JobName.NoticeExtendPackage, Cron.HourInterval(1), allTenants: true);
            //JobExtensions.SetRecurringJob<WeeklyReportWorker>(WorkerSettings.JobName.WeeklyReport, Cron.Weekly(DayOfWeek.Monday, 2, 0), allTenants: true);
            JobExtensions.SetRecurringJob<TreasureZMatterWorker>(WorkerSettings.JobName.TreasureZMatter, Cron.Weekly(DayOfWeek.Wednesday, 9, 0), allTenants: true);
            JobExtensions.SetRecurringJob<EnergyOfPlanetWorker>(WorkerSettings.JobName.EnergyOfPlanet, Cron.Weekly(DayOfWeek.Sunday, 9, 0), allTenants: true);
            //JobExtensions.SetRecurringJob<JobActiveEventWorker>(WorkerSettings.JobName.ActiveEvent, Cron.Daily(17, 0), allTenants: true);
            JobExtensions.SetRecurringJob<JobRunEventsWorker>(WorkerSettings.JobName.RunEvents, Cron.Daily(17, 0), allTenants: true);
            JobExtensions.SetRecurringJob<SendWeeklyReportWorker>(WorkerSettings.JobName.SendWeeklyReport, Cron.Weekly(DayOfWeek.Monday, 2, 0), allTenants: true);
            JobExtensions.SetRecurringJob<AggregateDataWeeklyReportWorker>(WorkerSettings.JobName.AggregateDataWeeklyReport, Cron.Weekly(DayOfWeek.Sunday, 17, 0), allTenants: true);
            JobExtensions.SetRecurringJob<WeeklySnapShotLeaderBoardWorker>(WorkerSettings.JobName.WeeklySnapShot, Cron.Weekly(DayOfWeek.Sunday, 23, 30), EnumCountryKey.Vietnam.FindSystemTimeZoneInfo(), allTenants: true);
            JobExtensions.SetRecurringJob<CheckUserDeletionWorker>(WorkerSettings.JobName.CheckUserDeletionJob, Cron.Daily(), allTenants: true);
            //JobExtensions.SetRecurringJob<TestWorker>(WorkerSettings.JobName.TestWorkerJob, Cron.Daily, allTenants: true);
            JobExtensions.SetRecurringJob<ChooseDailyQuizWinnersWorker>(WorkerSettings.JobName.ChooseDailyQuizWinners, Cron.Daily(13, 59), allTenants: true);
            JobExtensions.SetRecurringJob<AggregateDataStudentsInEventWorker>(WorkerSettings.JobName.AggregateDataStudentsInEvent, Cron.Daily(17, 1), allTenants: true);

            JobExtensions.SetRecurringJob<PushNoticeWorker, PushNoticeTime>(WorkerSettings.JobName.PushNotice, Cron.Hourly(), new PushNoticeTime(EnumPushNoticeTimeType.EveryHour), allTenants: true);
            JobExtensions.SetRecurringJob<PushNoticeWorker, PushNoticeTime>(WorkerSettings.JobName.PushNotice, Cron.Daily(0, 30), new PushNoticeTime(EnumPushNoticeTimeType.At07h30), allTenants: true);
            JobExtensions.SetRecurringJob<PushNoticeWorker, PushNoticeTime>(WorkerSettings.JobName.PushNotice, Cron.Daily(5, 0), new PushNoticeTime(EnumPushNoticeTimeType.At12h00), allTenants: true);
            JobExtensions.SetRecurringJob<PushNoticeWorker, PushNoticeTime>(WorkerSettings.JobName.PushNotice, Cron.Daily(10, 30), new PushNoticeTime(EnumPushNoticeTimeType.At17h30), allTenants: true);
            JobExtensions.SetRecurringJob<PushNoticeWorker, PushNoticeTime>(WorkerSettings.JobName.PushNotice, Cron.Daily(12, 30), new PushNoticeTime(EnumPushNoticeTimeType.At19h30), allTenants: true);
        }
    }
}

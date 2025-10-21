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
            //JobExtensions.SetRecurringJob<AssignmentScheduleWorker>(WorkerSettings.JobName.AssignmentScheduleJob, Cron.MinuteInterval(15));
            JobExtensions.SetRecurringJob<EndTrialRegistrationWorker>(WorkerSettings.JobName.EndTrialRegistrationJob, Cron.Daily());
            //JobExtensions.SetRecurringJob<UpdateTeacherGradingInClassForumAndMockTestWorker>(WorkerSettings.JobName.UpdateOcCheckInClassForumResultJob, Cron.MinuteInterval(15));
            JobExtensions.SetRecurringJob<SyncStudentShieldEveryDayWorker>(WorkerSettings.JobName.SyncStudentShieldEveryDayJob, Cron.Daily());
            //JobExtensions.SetRecurringJob<UpdateOcCheckInClassForumResultWorker>(WorkerSettings.JobName.UpdateTeacherGradingInClassForumAndMockTestJob, Cron.MinuteInterval(15));
            JobExtensions.SetRecurringJob<LeaderBoardWorker>(WorkerSettings.JobName.LeaderBoardJob, Cron.HourInterval(1));
            JobExtensions.SetRecurringJob<NoticeAccessTimeWorker>(WorkerSettings.JobName.NoticeAccessTime, Cron.HourInterval(1));
            JobExtensions.SetRecurringJob<NoticeExtendPackageWorker>(WorkerSettings.JobName.NoticeExtendPackage, Cron.HourInterval(1));
            //JobExtensions.SetRecurringJob<WeeklyReportWorker>(WorkerSettings.JobName.WeeklyReport, Cron.Weekly(DayOfWeek.Monday, 2, 0));
            JobExtensions.SetRecurringJob<TreasureZMatterWorker>(WorkerSettings.JobName.TreasureZMatter, Cron.Weekly(DayOfWeek.Wednesday, 9, 0));
            JobExtensions.SetRecurringJob<EnergyOfPlanetWorker>(WorkerSettings.JobName.EnergyOfPlanet, Cron.Weekly(DayOfWeek.Sunday, 9, 0));
            //JobExtensions.SetRecurringJob<JobActiveEventWorker>(WorkerSettings.JobName.ActiveEvent, Cron.Daily(17, 0));
            JobExtensions.SetRecurringJob<JobRunEventsWorker>(WorkerSettings.JobName.RunEvents, Cron.Daily(17, 0));
            JobExtensions.SetRecurringJob<SendWeeklyReportWorker>(WorkerSettings.JobName.SendWeeklyReport, Cron.Weekly(DayOfWeek.Monday, 2, 0));
            JobExtensions.SetRecurringJob<AggregateDataWeeklyReportWorker>(WorkerSettings.JobName.AggregateDataWeeklyReport, Cron.Weekly(DayOfWeek.Sunday, 17, 0));
            JobExtensions.SetRecurringJob<WeeklySnapShotLeaderBoardWorker>(WorkerSettings.JobName.WeeklySnapShot, Cron.Weekly(DayOfWeek.Sunday, 23, 30), EnumCountryKey.Vietnam.FindSystemTimeZoneInfo());
            JobExtensions.SetRecurringJob<CheckUserDeletionWorker>(WorkerSettings.JobName.CheckUserDeletionJob, Cron.Daily());
            //JobExtensions.SetRecurringJob<TestWorker>(WorkerSettings.JobName.TestWorkerJob, Cron.Daily);
            JobExtensions.SetRecurringJob<ChooseDailyQuizWinnersWorker>(WorkerSettings.JobName.ChooseDailyQuizWinners, Cron.Daily(13, 59));
            JobExtensions.SetRecurringJob<AggregateDataStudentsInEventWorker>(WorkerSettings.JobName.AggregateDataStudentsInEvent, Cron.Daily(17, 1));
            JobExtensions.SetRecurringJob<JobStudentAggregateWorker>(WorkerSettings.JobName.JobStudentAggregate, Cron.Weekly(DayOfWeek.Monday, 0, 0), EnumCountryKey.Vietnam.FindSystemTimeZoneInfo());

            JobExtensions.SetRecurringJob<PushNoticeWorker, PushNoticeTime>(WorkerSettings.JobName.PushNoticeEveryHour, Cron.Hourly(), new PushNoticeTime(EnumPushNoticeTimeType.EveryHour));
            JobExtensions.SetRecurringJob<PushNoticeWorker, PushNoticeTime>(WorkerSettings.JobName.PushNoticeAt07h30, Cron.Daily(0, 30), new PushNoticeTime(EnumPushNoticeTimeType.At07h30));
            JobExtensions.SetRecurringJob<PushNoticeWorker, PushNoticeTime>(WorkerSettings.JobName.PushNoticeAt12h00, Cron.Daily(5, 0), new PushNoticeTime(EnumPushNoticeTimeType.At12h00));
            JobExtensions.SetRecurringJob<PushNoticeWorker, PushNoticeTime>(WorkerSettings.JobName.PushNoticeAt17h30, Cron.Daily(10, 30), new PushNoticeTime(EnumPushNoticeTimeType.At17h30));
            JobExtensions.SetRecurringJob<PushNoticeWorker, PushNoticeTime>(WorkerSettings.JobName.PushNoticeAt19h30, Cron.Daily(12, 30), new PushNoticeTime(EnumPushNoticeTimeType.At19h30));
            JobExtensions.SetRecurringJob<NotifyWeeklyReportCourseTargetWorker>(WorkerSettings.JobName.NotifyWeeklyReportCourseTarget, Cron.Weekly(DayOfWeek.Monday, 8, 0), EnumCountryKey.Vietnam.FindSystemTimeZoneInfo());
            JobExtensions.SetRecurringJob<NotifyWeeklyCourseGoalTargetWorker>(WorkerSettings.JobName.NotifyWeeklyCourseGoalTarget, Cron.Weekly(DayOfWeek.Monday, 9, 0), EnumCountryKey.Vietnam.FindSystemTimeZoneInfo());
        }
    }
}

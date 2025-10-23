// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Constants;
using Fsel.Common.ValueSettings;
using Fsel.Core.Extensions;
using Fsel.Hangfire.Application.Queues.Consumers;
using Fsel.Hangfire.Application.Queues.Publishers;
using Fsel.Hangfire.Host.Jobs;
using Fsel.Shared.Constants;
using Hangfire;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

var appSetting = builder.AddAppSettings<BaseAppSetting>();
builder.AddServices(appSetting);
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);

builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString(Settings.DefaultConnection)));
builder.Services.AddHangfireServer();

builder.Services.AddScoped<UpdateClassLiveAssignmentPublisher>();
builder.Services.AddScoped<UpdateOcCheckInClassForumResultPublisher>();
builder.Services.AddScoped<UpdateTeacherGradingInClassForumAndMockTestPublisher>();
builder.Services.AddScoped<SyncStudentShieldEveryDayPublisher>();
builder.Services.AddScoped<LeaderBoardPublisher>();
builder.Services.AddScoped<CompleteTestWhenTimeOutPublisher>();
builder.Services.AddScoped<ReviewFselPublisher>();
builder.Services.AddScoped<NoticeAccessTimePublisher>();
builder.Services.AddScoped<NoticeExtendPackagePublisher>();
builder.Services.AddScoped<SendWeeklyReportPublisher>();
builder.Services.AddScoped<WeeklySnapShotLeaderBoardPublisher>();
builder.Services.AddScoped<UpdateStatusTrialStudentPublisher>();
builder.Services.AddScoped<RetryMockTestPublisher>();
builder.Services.AddScoped<RetryClassForumPublisher>();
builder.Services.AddScoped<UpdateClassForumResultToExpiredTimePublisher>();
builder.Services.AddScoped<JobActiveEventPublisher>();
builder.Services.AddScoped<JobRunEventsPublisher>();
builder.Services.AddScoped<CheckUserDeletionPublisher>();
builder.Services.AddScoped<WeeklyNoticePublisher>();
builder.Services.AddScoped<ChooseDailyQuizWinnersPublisher>();
builder.Services.AddScoped<AggregateDataStudentsInEventPublisher>();
builder.Services.AddScoped<AggregateDataWeeklyReportPublisher>();
builder.Services.AddScoped<PushNoticePublisher>();
builder.AddMassTransit(appSetting,
queues: new Dictionary<string, Type>
{
    { QueueSettings.LmsQueue.NameQueue.SetTimeToCompleteTest, typeof(SetTimeToCompleteTestConsumer) },
    { QueueSettings.LmsQueue.NameQueue.SetTimeRetryMockTest, typeof(SetTimeToRetryMockTestConsumer) },
    { QueueSettings.LmsQueue.NameQueue.SetTimeRetryClassForum, typeof(SetTimeToRetryClassForumConsumer) },
    { QueueSettings.SystemQueue.NameQueue.SetCompleteApprovalPostTimeOut, typeof(SetTimeToCompleteApprovalConsumer) },
    //{ QueueSettings.UserQueue.NameQueue.SetTimeToSendReviewFsel, typeof(SetTimeToReviewFselConsumer) },
    { QueueSettings.LmsQueue.NameQueue.SetTimeClassForumDone, typeof(SetTimeToClassForumApprovalConsumer) },
});
var app = builder.Build();

app.UseServices();
app.UseHangfireDashboards();
RecurringJobBase.Setup();
app.Run();

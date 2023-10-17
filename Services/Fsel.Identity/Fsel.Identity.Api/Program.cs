// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Extensions;
using Fsel.Identity.Application.Queues.Consumers;
using Fsel.Identity.Application.Queues.Publishers;
using Fsel.Identity.Application.Services;
using Fsel.Identity.Application.Services.InteractionService;
using Fsel.Identity.Application.Services.LmsCourseService;
using Fsel.Identity.Application.Services.OrderService;
using Fsel.Identity.Application.Services.SystemService;
using Fsel.Identity.Application.Services.TrainingService;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Infrastructure;
using Fsel.Identity.Infrastructure.Repositories;
using Fsel.Identity.Infrastructure.ValueSettings;
using Fsel.Shared.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<UserDbContext>();

builder.AddIdentity<User, Role, UserDbContext>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserTokenRepository, UserTokenRepository>();
builder.Services.AddScoped<IHumanRepository, HumanRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<IParentRepository, ParentRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IUserOtpCodeRepository, UserOtpCodeRepository>();
builder.Services.AddScoped<IParentStudentRepository, ParentStudentRepository>();
builder.Services.AddScoped<IStudentDailyStreakRepository, StudentDailyStreakRepository>();
builder.Services.AddScoped<ICSORepository, CSORepository>();
builder.Services.AddScoped<ITeacherBankAccountRepository, TeacherBankAccountRepository>();
builder.Services.AddScoped<IUserSettingRepository, UserSettingRepository>();
builder.Services.AddScoped<IPlatformRepository, PlatformRepository>();
builder.Services.AddScoped<IStudentRankingRepository, StudentRankingRepository>();
builder.Services.AddScoped<LeaderBoardPublisher>();
builder.Services.AddScoped<IUserPlatformRepository, UserPlatformRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IStudentFocusTimeRepository, StudentFocusTimeRepository>();

builder.AddRefitClients(typeof(ISenderService), appSetting?.Services?.SenderApiUrl);
builder.AddRefitClients(typeof(IOrderService), appSetting?.Services?.OrderApiUrl);
builder.AddRefitClients(typeof(IInteractionService), appSetting?.Services?.InteractionApiUrl);
builder.AddRefitClients(typeof(ITrainingService), appSetting?.Services?.ClassApiUrl);
builder.AddRefitClients(typeof(ILmsCourseService), appSetting?.Services?.LmsCourseApiUrl);
builder.AddRefitClients(typeof(ISystemService), appSetting?.Services?.SystemApiUrl);

builder.AddMassTransit(appSetting,
queues: new Dictionary<string, Type>
{
    { QueueSettings.UserQueue.NameQueue.SyncStudentShieldEveryDay, typeof(SyncStudentShieldForDailyStreakEveryDayConsumer) },
    { QueueSettings.UserQueue.NameQueue.UpdateStudentsDailyStreak, typeof(SyncStudentShieldForDailyStreakEveryDayConsumer) },
    { QueueSettings.UserQueue.NameQueue.LeaderBoard, typeof(LeaderBoardConsumer) }
});
var app = builder.Build();
app.UseServices();
app.Run();

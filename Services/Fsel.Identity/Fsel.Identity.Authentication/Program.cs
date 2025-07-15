// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Extensions;
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
using Fsel.Identity.Infrastructure.Common;
using Fsel.Identity.Infrastructure.Repositories;
using Fsel.Identity.Infrastructure.ValueSettings;

var builder = WebApplication.CreateBuilder(args);
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<UserDbContext>();

builder.AddIdentity<User, Role, UserDbContext>();
builder.AddAuthenticationIdentity();

//Repository
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserTokenRepository, UserTokenRepository>();
builder.Services.AddScoped<IHumanRepository, HumanRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<IParentRepository, ParentRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IUserOtpCodeRepository, UserOtpCodeRepository>();
builder.Services.AddScoped<IParentStudentRepository, ParentStudentRepository>();
builder.Services.AddScoped<ICSORepository, CSORepository>();
builder.Services.AddScoped<ITeacherBankAccountRepository, TeacherBankAccountRepository>();
builder.Services.AddScoped<IUserSettingRepository, UserSettingRepository>();
builder.Services.AddScoped<IPlatformRepository, PlatformRepository>();
builder.Services.AddScoped<IUserPlatformRepository, UserPlatformRepository>();
builder.Services.AddScoped<IStudentDailyStreakRepository, StudentDailyStreakRepository>();
builder.Services.AddScoped<IStudentRankingRepository, StudentRankingRepository>();
builder.Services.AddScoped<IStudentTrialRegistrationRepository, StudentTrialRegistrationRepository>();
builder.Services.AddScoped<IUserCourseSettingRepository, UserCourseSettingRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IStudentFocusTimeRepository, StudentFocusTimeRepository>();
builder.Services.AddScoped<IStudentCompetitionSnapShotRepository, StudentCompetitionSnapShotRepository>();
builder.Services.AddScoped<ICompetitionEventsRepository, CompetitionEventsRepository>();
builder.Services.AddScoped<IEventRegistrationRepository, EventRegistrationRepository>();
builder.Services.AddScoped<IStudentCompetitionEventsRepository, StudentCompetitionEventRepository>();
builder.Services.AddScoped<IStudentRankingEventRepository, StudentRankingEventRepository>();
builder.Services.AddScoped<IUserReferralRepository, UserReferralRepository>();
builder.Services.AddScoped<IEventRegistrationRepository, EventRegistrationRepository>();
builder.Services.AddScoped<IUserDeletionRepository, UserDeletionRepository>();
builder.Services.AddScoped<IUserSchoolRepository, UserSchoolRepository>();
builder.Services.AddScoped<ISchoolImportHistoryRepository, SchoolImportHistoryRepository>();
builder.Services.AddScoped<IPermissionGroupRepository, PermissionGroupRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IRoleClaimRepository, RoleClaimRepository>();
builder.Services.AddScoped<IUserGroupRepository, UserGroupRepository>();
builder.Services.AddScoped<IUserGroupMemberShipRepository, UserGroupMemberShipRepository>();
builder.Services.AddScoped<IEventManagerRepository, EventManagerRepository>();
builder.Services.AddScoped<IStudentEventLearningRecordRepository, StudentEventLearningRecordRepository>();
builder.Services.AddScoped<IStudentEditHistoryRepository, StudentEditHistoryRepository>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
// Queue
builder.Services.AddScoped<LeaderBoardPublisher>();
builder.Services.AddScoped<QuestBoardPublisher>();
builder.Services.AddScoped<NotificationMessagePublisher>();
builder.Services.AddScoped<CreateTokenHistoryPublisher>();
builder.Services.AddScoped<CreateStudentsFromFilePublisher>();
builder.Services.AddScoped<SendStudentsFromFilePublisher>();

//Common
builder.Services.AddScoped<SaveOtpCodeConverter>();

//Refit
builder.AddRefitClients(typeof(ISenderService), appSetting?.Services?.SenderApiUrl);
builder.AddRefitClients(typeof(IOrderService), appSetting?.Services?.OrderApiUrl);
builder.AddRefitClients(typeof(IInteractionService), appSetting?.Services?.InteractionApiUrl);
builder.AddRefitClients(typeof(ITrainingService), appSetting?.Services?.ClassApiUrl);
builder.AddRefitClients(typeof(ILmsCourseService), appSetting?.Services?.LmsCourseApiUrl);
builder.AddRefitClients(typeof(ISystemService), appSetting?.Services?.SystemApiUrl);

builder.AddMassTransit(appSetting);
//App config
var app = builder.Build();
app.UseServices();
app.Run();
// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Extensions;
using Fsel.Shared.Constants;
using Fsel.System.Application.Queues.Consumers;
using Fsel.System.Application.Services.CourseServices;
using Fsel.System.Application.Services.OrderServices;
using Fsel.System.Application.Services.UserServices;
using Fsel.System.Domain.IRepositories;
using Fsel.System.Infrastructure;
using Fsel.System.Infrastructure.Repositories;
using Fsel.System.Infrastructure.ValueSettings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices();
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<SystemDbContext>();

builder.Services.AddScoped<ILiveTimeFrameRepository, LiveTimeFrameRepository>();
builder.Services.AddScoped<ICourseTimeConfigRepository, CourseTimeConfigRepository>();
builder.Services.AddScoped<IForbiddenWordRepository, ForbiddenWordRepository>();
builder.Services.AddScoped<ITeachingCostRepository, TeachingCostRepository>();
builder.Services.AddScoped<IReferralDiscountConfigRepository, ReferralDiscountConfigRepository>();
builder.Services.AddScoped<ILogActionRepository, LogActionRepository>();
builder.Services.AddScoped<IQuestBoardRepository, QuestBoardRepository>();
builder.Services.AddScoped<IQuestBoardConfigRepository, QuestBoardConfigRepository>();
builder.Services.AddScoped<IQuestBoardStudentRepository, QuestBoardStudentRepository>();
builder.AddRefitClients(typeof(IUserService), appSetting?.Services?.UserApiUrl);
builder.AddRefitClients(typeof(ICourseService), appSetting?.Services?.LmsCourseApiUrl);
builder.AddRefitClients(typeof(IOrderService), appSetting?.Services?.OrderApiUrl);

builder.AddMassTransit(appSetting,
queues: new Dictionary<string, Type>
{
    { QueueSettings.RealtimeQueue.NameQueue.QuestBoardMainFinish, typeof(QuestBoardMainFinishConsumer) },
});

var app = builder.Build();
app.UseServices();
app.Run();

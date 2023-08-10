// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ValueSettings;
using Fsel.Core.Extensions;
using Fsel.System.Application.Services.CourseServices;
using Fsel.System.Application.Services.UserServices;
using Fsel.System.Domain.IRepositories;
using Fsel.System.Infrastructure;
using Fsel.System.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<BaseAppSetting>();
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
builder.AddRefitClients(typeof(IUserService), appSetting?.Services?.UserApiUrl);
builder.AddRefitClients(typeof(ICourseService), appSetting?.Services?.UserApiUrl);
builder.AddRefitClients(typeof(IUserService), appSetting?.Services?.UserApiUrl);
var app = builder.Build();

app.UseServices();
app.Run();

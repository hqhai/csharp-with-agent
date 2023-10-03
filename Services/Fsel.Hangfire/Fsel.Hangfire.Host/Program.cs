// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Constants;
using Fsel.Common.ValueSettings;
using Fsel.Core.Extensions;
using Fsel.Hangfire.Application.Queues.Publishers;
using Fsel.Hangfire.Host.Jobs;
using Hangfire;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

var appSetting = builder.AddAppSettings<BaseAppSetting>();
builder.AddServices(appSetting);
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddMassTransit(appSetting);

builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString(Settings.DefaultConnection)));
builder.Services.AddHangfireServer();
builder.Services.AddScoped<UpdateClassLiveAssignmentPublisher>();
builder.Services.AddScoped<UpdateOcCheckInClassForumResultPublisher>();
builder.Services.AddScoped<UpdateTeacherGradingInClassForumAndMockTestPublisher>();
builder.Services.AddScoped<UpdateStudentsDailyStreakPublisher>();
var app = builder.Build();

app.UseServices();
app.UseHangfireDashboards();
RecurringJobBase.Setup();
app.Run();

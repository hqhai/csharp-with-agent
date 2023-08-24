// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Constants;
using Fsel.Common.ValueSettings;
using Fsel.Core.Extensions;
using Fsel.Hangfire.Host.Jobs;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

var appSetting = builder.AddAppSettings<BaseAppSetting>();
builder.AddServices();
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);

builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString(Settings.DefaultConnection)));
builder.Services.AddHangfireServer();

var app = builder.Build();

app.UseHangfireDashboard();
RecurringJobBase.Setup();

app.UseServices();
app.Run();

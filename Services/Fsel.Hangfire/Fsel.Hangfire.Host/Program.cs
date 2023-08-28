// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Constants;
using Fsel.Common.ValueSettings;
using Fsel.Core.Base.Interfaces;
using Fsel.Core.Extensions;
using Fsel.Hangfire.Application.Queues.Publishers;
using Fsel.Hangfire.Host.Jobs;
using Fsel.Shared.Constants;
using Hangfire;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

var appSetting = builder.AddAppSettings<BaseAppSetting>();
builder.AddServices();
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);

builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString(Settings.DefaultConnection)));
builder.Services.AddHangfireServer();
builder.Services.AddScoped<UpdateClassLiveAssignmentPublisher>();

var app = builder.Build();

app.UseServices();
app.UseHangfireDashboards();
builder.AddMassTransit(appSetting,
queues: new Dictionary<string, Type>
{
    { QueueSettings.RealtimeQueue.NameQueue.UpdateClassLiveAssignment, typeof(UpdateClassLiveAssignmentPublisher) }
});
RecurringJobBase.Setup();
app.Run();

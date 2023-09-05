// Copyright (c) Atlantic. All rights reserved.

using Fsel.Notification.Domain.IRepositories;
using Fsel.Notification.Infrastructure;
using Fsel.Notification.Infrastructure.ValueSettings;
using Fsel.Notification.Application.Services;
using Fsel.Core.Extensions;
using Fsel.Notification.Infrastructure.Repositories;
using Fsel.Notification.Application.Queues.Publishers;

var builder = WebApplication.CreateBuilder(args);

var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices();
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<NotificationsDBContext>();

builder.Services.AddScoped<INotificationsRepository, NotificationsRepository>();
builder.Services.AddScoped<INotificationTypeRepository, NotificationTypeRepository>();
builder.Services.AddScoped<INotificationRemindRepository, NotificationRemindRepository>();
builder.Services.AddScoped<NotificationMessagePublisher>();

builder.AddRefitClients(typeof(IUserService), appSetting?.Services?.UserApiUrl);

builder.AddMassTransit(appSetting);

var app = builder.Build();
app.UseServices(appSetting);
app.Run();



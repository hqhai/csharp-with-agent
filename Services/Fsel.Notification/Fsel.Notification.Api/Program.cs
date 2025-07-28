// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Extensions;
using Fsel.Notification.Application.Queues.Consumers;
using Fsel.Notification.Application.Queues.Publishers;
using Fsel.Notification.Application.Services;
using Fsel.Notification.Domain.IRepositories;
using Fsel.Notification.Infrastructure;
using Fsel.Notification.Infrastructure.Repositories;
using Fsel.Notification.Infrastructure.ValueSettings;
using Fsel.Shared.Constants;

var builder = WebApplication.CreateBuilder(args);

var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<NotificationsDBContext, NotificationsReadDbContext>();

builder.Services.AddScoped<INotificationsRepository, NotificationsRepository>();
builder.Services.AddScoped<INotificationTypeRepository, NotificationTypeRepository>();
builder.Services.AddScoped<INotificationRemindRepository, NotificationRemindRepository>();
builder.Services.AddScoped<NotificationMessagePublisher>();
builder.AddRefitClients(typeof(IUserService), appSetting?.Services?.UserApiUrl);

builder.AddMassTransit(appSetting, queues:
new Dictionary<string, Type>
{
    { QueueSettings.NotificationQueue.NameQueue.DiscussionBoard, typeof(DiscussionBoardCommentConsumer) },
    { QueueSettings.InteractionQueue.NameQueue.InteractionAction, typeof(InterationActionConsumer) },
    { QueueSettings.InteractionQueue.NameQueue.SendNotification, typeof(SendNotificationConsumer) },
    { QueueSettings.OrderingQueue.NameQueue.SendNotification, typeof(SendNotificationConsumer) },
    { QueueSettings.LmsQueue.NameQueue.SendNotification, typeof(SendNotificationConsumer) },
    { QueueSettings.UserQueue.NameQueue.SendNotification, typeof(SendNotificationConsumer) },
});

var app = builder.Build();
app.UseServices();
app.Run();

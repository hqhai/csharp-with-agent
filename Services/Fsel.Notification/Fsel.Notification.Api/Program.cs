// Copyright (c) Atlantic. All rights reserved.

using Fsel.Notification.Domain.IRepositories;
using Fsel.Notification.Infrastructure;
using Fsel.Notification.Infrastructure.ValueSettings;
using Fsel.Notification.Application.Services;
using Fsel.Core.Extensions;
using Fsel.Notification.Infrastructure.Repositories;
using Fsel.Notification.Application.Queues.Publishers;
using Fsel.Shared.Constants;
using Fsel.Notification.Application.Queues.Consumers;

var builder = WebApplication.CreateBuilder(args);

var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<NotificationsDBContext>();

builder.Services.AddScoped<INotificationsRepository, NotificationsRepository>();
builder.Services.AddScoped<INotificationTypeRepository, NotificationTypeRepository>();
builder.Services.AddScoped<INotificationRemindRepository, NotificationRemindRepository>();
builder.Services.AddScoped<NotificationMessagePublisher>();
builder.AddRefitClients(typeof(IUserService), appSetting?.Services?.UserApiUrl);



builder.AddMassTransit(appSetting, queues:
new Dictionary<string, Type>
{
    { QueueSettings.NotificationQueue.NameQueue.DiscussionBoard, typeof(DiscussionBoardCommentConsumer) },
    { QueueSettings.InteractionQueue.NameQueue.ClassForum, typeof(InterationActionConsumer) },
    { QueueSettings.InteractionQueue.NameQueue.Comment, typeof(ClassForumCommentConsumer) },
});

var app = builder.Build();
app.UseServices(appSetting);
app.Run();



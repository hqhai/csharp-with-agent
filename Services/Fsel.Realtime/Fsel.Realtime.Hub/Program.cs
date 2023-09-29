// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ValueSettings;
using Fsel.Core.Extensions;
using Fsel.Realtime.Application.Hubs;
using Fsel.Realtime.Application.Queues.Consumers;
using Fsel.Shared.Constants;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

var appSetting = builder.AddAppSettings<BaseAppSetting>();
builder.AddServices(appSetting);
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);

builder.AddMassTransit(appSetting,
queues: new Dictionary<string, Type>
{
    { QueueSettings.RealtimeQueue.NameQueue.DiscussionBoard, typeof(DiscussionBoardConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.LeaderBoard, typeof(LeaderBoardConsumer) },
    { QueueSettings.NotificationQueue.NameQueue.Notification, typeof(NotificationConsumer) }
});

var app = builder.Build();
app.UseServices();
app.UseHubs<DiscussionBoardHub>(RealtimeSettings.DiscussionBoardHub.Pattern);
app.UseHubs<NotificationHub>(RealtimeSettings.NotificationHub.Pattern);
app.UseHubs<NotificationHub>(RealtimeSettings.LeaderBoardHub.Pattern);
app.Run();

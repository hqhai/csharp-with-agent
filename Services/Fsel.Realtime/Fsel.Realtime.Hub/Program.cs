// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ValueSettings;
using Fsel.Core.Extensions;
using Fsel.Realtime.Application.Hubs;
using Fsel.Realtime.Application.Queues.Consumers;
using Fsel.Shared.Constants;

var builder = WebApplication.CreateBuilder(args);

var appSetting = builder.AddAppSettings<BaseAppSetting>();
builder.AddServices();
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);

builder.AddRabbitMq(appSetting, new Dictionary<string, Type>
{
    { QueueSettings.RealtimeQueue.NameQueue.DiscussionBoard, typeof(DiscussionBoardConsumer) }
});
builder.AddHubs<DiscussionBoardHub>();

var app = builder.Build();
app.UseServices();
app.UseHubs<DiscussionBoardHub>(RealtimeSettings.DiscussionBoardHub.Pattern);
app.Run();

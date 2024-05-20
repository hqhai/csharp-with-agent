// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ValueSettings;
using Fsel.Core.Extensions;
using Fsel.Realtime.Application.Hubs;
using Fsel.Realtime.Application.Queues.Consumers;
using Fsel.Realtime.Application.Queues.Publishers;
using Fsel.Shared.Constants;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

var appSetting = builder.AddAppSettings<BaseAppSetting>();
builder.AddServices(appSetting);
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.Services.AddScoped<FeatureAccessTimePublisher>();
builder.Services.AddScoped<SetTimeModulePublisher>();
builder.Services.AddScoped<ChatBotPublisher>();
builder.Services.AddScoped<SetTimeModuleHub>();

builder.AddMassTransit(appSetting,
queues: new Dictionary<string, Type>
{
    { QueueSettings.RealtimeQueue.NameQueue.DiscussionBoard, typeof(DiscussionBoardConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.LeaderBoard, typeof(LeaderBoardConsumer) },
    { QueueSettings.NotificationQueue.NameQueue.Notification, typeof(NotificationConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.AIFeedBack, typeof(AIFeedBackConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.MockTestWriting, typeof(MockTestAIFeedBackConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.ChatBotRealTime, typeof(ChatBotConsumer) },
    { QueueSettings.LmsQueue.NameQueue.DisconnectSocketCalculateTime, typeof(DisconnectSocketCalculateTimeConsumer) },
});

var app = builder.Build();
app.UseServices();
app.UseHubs<DiscussionBoardHub>(RealtimeSettings.DiscussionBoardHub.Pattern);
app.UseHubs<NotificationHub>(RealtimeSettings.NotificationHub.Pattern);
app.UseHubs<LeaderBoardHub>(RealtimeSettings.LeaderBoardHub.Pattern);

app.UseHubs<ClassForumAIFeedBackHub>(RealtimeSettings.ClassForumAIFeedBackHub.Pattern);
app.UseHubs<MockTestWritingHub>(RealtimeSettings.MockTestWritingAIFeedBackHub.Pattern);
app.UseHubs<FeatureAccessTimeHub>(RealtimeSettings.FeatureAccessTimeHub.Pattern);
app.UseHubs<SetTimeModuleHub>(RealtimeSettings.SetTimeModuleHub.Pattern);
app.UseHubs<ChatBotHub>(RealtimeSettings.ChatBotHub.Pattern);
app.Run();

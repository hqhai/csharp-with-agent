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
builder.Services.AddScoped<GetTimeModulePublisher>();
builder.Services.AddScoped<ChatBotPublisher>();
builder.Services.AddScoped<TechieActionPublisher>();
builder.Services.AddScoped<DictionaryPublisher>();
builder.Services.AddScoped<SetTimeExamPracticePublisher>();
builder.Services.AddScoped<GetTimeExamPracticePublisher>();

builder.Services.AddScoped<SetTimeModuleHub>();
builder.Services.AddScoped<TechieHub>();
builder.Services.AddScoped<BannerHub>();
builder.Services.AddScoped<BannerPublisher>();
builder.Services.AddScoped<TranscriptHub>();
builder.Services.AddScoped<BuyBlindBoxHub>();
builder.Services.AddScoped<DictionaryHub>();
builder.Services.AddScoped<SetTimeExamPracticeHub>();

builder.AddMassTransit(appSetting,
multicastQueues: new Dictionary<string, Type>
{
    { QueueSettings.RealtimeQueue.NameQueue.DiscussionBoard, typeof(DiscussionBoardConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.LeaderBoard, typeof(LeaderBoardConsumer) },
    { QueueSettings.NotificationQueue.NameQueue.Notification, typeof(NotificationConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.AIFeedBack, typeof(AIFeedBackConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.MockTestWriting, typeof(MockTestAIFeedBackConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.ChatBotRealTime, typeof(ChatBotConsumer) },
    { QueueSettings.LmsQueue.NameQueue.DisconnectSocketCalculateTime, typeof(DisconnectSocketCalculateTimeConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.MockTestSpeaking, typeof(MockTestAISpeakingConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.ExamPracticeSpeaking, typeof(ExamPracticeAISpeakingConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.ExamPracticeWriting, typeof(ExamPracticeAIFeedBackConsumer) },
    { QueueSettings.LmsQueue.NameQueue.GetTimeModule, typeof(GetTimeModuleConsumer) },
    { QueueSettings.SystemQueue.NameQueue.Techie, typeof(StudentTechieConsumer) },
    { QueueSettings.OrderingQueue.NameQueue.ChangeStatusOrder, typeof(ChangeStatusOrderConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.SendStudentsFromFile, typeof(SendStudentsFromFileConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.BannerRealTime, typeof(BannerConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.SpeechToTextRealTime, typeof(SpeechToTextConsumer) },
    { QueueSettings.SystemQueue.NameQueue.SendNotifyBuyBlindBox, typeof(SendNotifyBuyBlindBoxConsumer) },
    { QueueSettings.SystemQueue.NameQueue.SendDictionary, typeof(SendDictionaryConsumer) },
    { QueueSettings.ExamPracticeQueue.NameQueue.GetTimeExamPractice, typeof(GetTimeExamPracticeConsumer) },
});

var app = builder.Build();
app.UseServices();
app.UseHubs<DiscussionBoardHub>(RealtimeSettings.DiscussionBoardHub.Pattern);
app.UseHubs<NotificationHub>(RealtimeSettings.NotificationHub.Pattern);
app.UseHubs<LeaderBoardHub>(RealtimeSettings.LeaderBoardHub.Pattern);
app.UseHubs<ClassForumAIFeedBackHub>(RealtimeSettings.ClassForumAIFeedBackHub.Pattern);
app.UseHubs<MockTestWritingHub>(RealtimeSettings.MockTestWritingAIFeedBackHub.Pattern);
app.UseHubs<MockTestSpeakingHub>(RealtimeSettings.MockTestSpeakingAIFeedBackHub.Pattern);
app.UseHubs<FeatureAccessTimeHub>(RealtimeSettings.FeatureAccessTimeHub.Pattern);
app.UseHubs<SetTimeModuleHub>(RealtimeSettings.SetTimeModuleHub.Pattern);
app.UseHubs<ChatBotHub>(RealtimeSettings.ChatBotHub.Pattern);
app.UseHubs<TechieHub>(RealtimeSettings.TechieHub.Pattern);
app.UseHubs<PaymentHub>(RealtimeSettings.PaymentHub.Pattern);
app.UseHubs<SendStudentsFromFileHub>(RealtimeSettings.SendStudentsFromFileHub.Pattern);
app.UseHubs<BannerHub>(RealtimeSettings.BannerHub.Pattern);
app.UseHubs<TranscriptHub>(RealtimeSettings.TranscriptHub.Pattern);
app.UseHubs<BuyBlindBoxHub>(RealtimeSettings.SendNotifyBuyBlindBoxHub.Pattern);
app.UseHubs<DictionaryHub>(RealtimeSettings.SendDictionaryHub.Pattern);
app.UseHubs<ExamPracticeSpeakingHub>(RealtimeSettings.ExamPracticeSpeakingAIFeedBackHub.Pattern);
app.UseHubs<ExamPracticeWritingHub>(RealtimeSettings.ExamPracticeWritingAIFeedBackHub.Pattern);
app.UseHubs<SetTimeExamPracticeHub>(RealtimeSettings.SetTimeExamPracticeHub.Pattern);

app.Run();

// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Extensions;
using Fsel.Realtime.Application.Hubs;
using Fsel.Realtime.Application.Hubs.Test;
using Fsel.Realtime.Application.Queues.Consumers;
using Fsel.Realtime.Application.Queues.Consumers.Test;
using Fsel.Realtime.Application.Queues.Publishers;
using Fsel.Realtime.Application.Services.SpeechToText;
using Fsel.Realtime.Application.ValueSettings;
using Fsel.Realtime.Domain.SpeechToTextModel;
using Fsel.Realtime.Infrastructure.Services;
using Fsel.Shared.Constants;

var builder = WebApplication.CreateBuilder(args);

var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddOpenIdSwaggerGens(appSetting);
builder.AddOpenIdAuthenticationJwtBearers(appSetting);
builder.Services.AddScoped<FeatureAccessTimePublisher>();
builder.Services.AddScoped<SetTimeModulePublisher>();
builder.Services.AddScoped<GetTimeModulePublisher>();
builder.Services.AddScoped<ChatBotPublisher>();
builder.Services.AddScoped<TechieActionPublisher>();
builder.Services.AddScoped<DictionaryPublisher>();
builder.Services.AddScoped<SetTimeExamPracticePublisher>();
builder.Services.AddScoped<GetTimeExamPracticePublisher>();
builder.Services.AddScoped<QuestionTypePublisher>();
builder.Services.AddScoped<AITranslationRequestPublisher>();
builder.Services.AddScoped<TranslationHub>();

builder.Services.AddScoped<SetTimeModuleHub>();
builder.Services.AddScoped<TechieHub>();
builder.Services.AddScoped<BannerHub>();
builder.Services.AddScoped<BannerPublisher>();
builder.Services.AddScoped<TranscriptHub>();
builder.Services.AddScoped<BuyBlindBoxHub>();
builder.Services.AddScoped<DictionaryHub>();
builder.Services.AddScoped<SetTimeExamPracticeHub>();
builder.Services.AddScoped<QuestionTypeHub>();
builder.Services.AddScoped<SendStudentsFromFileHub>();
builder.Services.AddScoped<TestSpeakingHub>();
builder.Services.AddScoped<TestWritingHub>();
builder.Services.AddScoped<SpeechToTextHub>();

// Speech-to-Text services (SOLID: DIP - depend on abstractions)
// Use Singleton to share session dictionary across all SignalR calls
builder.Services.AddSingleton<ISpeechRecognitionEventHandler, SpeechRecognitionEventNotifier>();
builder.Services.AddSingleton<ISpeechRecognitionService, AzureSpeechRecognitionService>();

// Refit clients for storage service
builder.AddRefitClients(typeof(IStorageService), appSetting?.Services?.StorageApiUrl);

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
    { QueueSettings.RealtimeQueue.NameQueue.TestSpeaking, typeof(TestAISpeakingConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.TestWriting, typeof(TestAIFeedBackConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.AITranslationResponse, typeof(AITranslationResultConsumer) },
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
app.UseHubs<QuestionTypeHub>(RealtimeSettings.QuestionTypeHub.Pattern);
app.UseHubs<TestWritingHub>(RealtimeSettings.TestWritingAIFeedBackHub.Pattern);
app.UseHubs<TestSpeakingHub>(RealtimeSettings.TestSpeakingAIFeedBackHub.Pattern);
app.UseHubs<TranslationHub>(RealtimeSettings.TranslationHub.Pattern);
app.UseHubs<SpeechToTextHub>(RealtimeSettings.SpeechToTextHub.Pattern);

app.Run();

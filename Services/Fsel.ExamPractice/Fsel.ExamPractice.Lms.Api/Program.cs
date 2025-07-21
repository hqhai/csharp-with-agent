// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Constants;
using Fsel.Core.Extensions;
using Fsel.ExamPractice.Domain.IRepositories;
using Fsel.ExamPractice.Infrastructure;
using Fsel.ExamPractice.Infrastructure.Common;
using Fsel.ExamPractice.Infrastructure.Repositories;
using Fsel.ExamPractice.Infrastructure.ValueSettings;
using Fsel.ExamPractice.Lms.Application.Queues.Consumers;
using Fsel.ExamPractice.Lms.Application.Queues.Publishers;
using Fsel.ExamPractice.Lms.Application.Services.AiService;
using Fsel.ExamPractice.Lms.Application.Services.AiService.SpeakingAIService;
using Fsel.ExamPractice.Lms.Application.Services.AIService.SpeakingAIService;
using Fsel.ExamPractice.Lms.Application.Services.AIService.SpeakingAIService.Interface;
using Fsel.ExamPractice.Lms.Application.Services.UserServices;
using Fsel.Shared.Constants;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<ExamPracticesDBContext, ExamPracticesReadDBContext>();

builder.Services.AddScoped<IExamPracticeAnswerRepository, ExamPracticeAnswerRepository>();
builder.Services.AddScoped<IExamPracticeRepository, ExamPracticeRepository>();
builder.Services.AddScoped<IExamPracticeResultRepository, ExamPracticeResultRepository>();
builder.Services.AddScoped<IExamPracticeRetryRepository, ExamPracticeRetryRepository>();
builder.Services.AddScoped<IExamPracticeSectionRepository, ExamPracticeSectionRepository>();
builder.Services.AddScoped<IExamPracticeSectionResultRepository, ExamPracticeSectionResultRepository>();
builder.Services.AddScoped<IExamPracticeScoreRepository, ExamPracticeScoreRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IExamPracticeAISettingRepository, ExamPracticeAISettingRepository>();
builder.Services.AddScoped<ISpeakingEvaluationAIService, SpeakingEvaluationAIService>();
builder.Services.AddScoped<ISpeakingAIService, SpeakingAIService>();
builder.Services.AddScoped<IProsodyScoreRepository, ProsodyScoreRepository>();

builder.Services.AddScoped<ExamPracticeHelper>();
builder.Services.AddScoped<ExamPracticeSectionHelper>();

builder.Services.AddScoped<SubmitExamPracticeCriteriaPublisher>();
builder.Services.AddScoped<SetTimeRetryExamPracticePublisher>();
builder.Services.AddScoped<GetTimeModulePublisher>();
builder.Services.AddScoped<SubmitSpeakingAIPublisher>();
builder.Services.AddScoped<SubmitExamPracticeAnswerPublisher>();
builder.Services.AddScoped<SubmitAiSpeakingAnswerPublisher>();

builder.AddRefitClients(typeof(IUserService), appSetting?.Services?.UserApiUrl);
builder.Services.AddRefitClient<IOpenAIService>().ConfigureHttpClient(delegate (IServiceProvider serviceProvider, HttpClient httpClient)
{
    httpClient.BaseAddress = new Uri(appSetting?.OpenAiConfig?.Uri ?? string.Empty);
    if (appSetting?.OpenAiConfig?.ApiKeys != null && appSetting.OpenAiConfig.ApiKeys!.Any())
    {
        var randomApiKey = appSetting.OpenAiConfig.ApiKeys[Random.Shared.Next(appSetting.OpenAiConfig.ApiKeys.Count)];
        httpClient.DefaultRequestHeaders.Add("Authorization", $"{Settings.Bearer} {randomApiKey}");
    }
});

builder.AddMassTransit(appSetting,
queues: new Dictionary<string, Type>
{
       { QueueSettings.RealtimeQueue.NameQueue.GetTimeExamPractice, typeof(GetTimeExamPracticeConsumer) },
       { QueueSettings.ExamPracticeQueue.NameQueue.ExamPracticeAnwserResponse, typeof(AiFeedBackResponseConsumer) },
       { QueueSettings.ExamPracticeQueue.NameQueue.SpeakingAI, typeof(SpeakingAIEvaluationConsumer) },
       { QueueSettings.ExamPracticeQueue.NameQueue.SetTimeExamPractice, typeof(SetTimeExamPracticeConsumer) },
});
var app = builder.Build();
app.UseServices();
app.Run();

// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Constants;
using Fsel.Core.Extensions;
using Fsel.Shared.Constants;
using Fsel.Storage.Application.Queues.Consumers;
using Fsel.Storage.Application.Queues.Publisher;
using Fsel.Storage.Application.Services.AmazonS3Services;
using Fsel.Storage.Application.Services.OpenAIServices;
using Fsel.Storage.Application.Services.SenderServices;
using Fsel.Storage.Infrastructure.ValueSettings;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddRefitClients(typeof(ISenderService), appSetting?.Services?.SenderApiUrl);
builder.Services.AddRefitClient<IOpenAIService>().ConfigureHttpClient(delegate (IServiceProvider serviceProvider, HttpClient httpClient)
{
    httpClient.BaseAddress = new Uri(appSetting?.OpenAiConfig?.Uri ?? string.Empty);
    if (!string.IsNullOrEmpty(appSetting?.OpenAiConfig?.ApiKey))
    {
        httpClient.DefaultRequestHeaders.Add("Authorization", $"{Settings.Bearer} {appSetting?.OpenAiConfig?.ApiKey}");
    }
});

builder.Services.AddScoped<IAmazonS3Service, AmazonS3Service>();
builder.Services.AddScoped<SpeechToTextPublisher>();
builder.Services.AddScoped<SpeechToTextAiPublisher>();

builder.AddMassTransit(appSetting,
queues: new Dictionary<string, Type>
{
  { QueueSettings.StorageQueue.NameQueue.SpeechToTextAi, typeof(SpeechToTextAiConsumer) }
});

var app = builder.Build();
app.UseServices();
app.Run();

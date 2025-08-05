// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Constants;
using Fsel.Core.Extensions;
using Fsel.Shared.Constants;
using Fsel.Storage.Application.Queues.Consumers;
using Fsel.Storage.Application.Queues.Publisher;
using Fsel.Storage.Application.Services.AmazonS3Services;
using Fsel.Storage.Application.Services.FFmpegServices;
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
builder.AddRefitClients(typeof(IFFmpegServices), appSetting?.Services?.FFmpegApiUrl);
builder.Services.AddRefitClient<IOpenAIService>().ConfigureHttpClient(delegate (IServiceProvider serviceProvider, HttpClient httpClient)
{
    httpClient.BaseAddress = new Uri(appSetting?.OpenAiConfig?.Uri ?? string.Empty);
    httpClient.Timeout = TimeSpan.FromMinutes(3);
    if (appSetting?.OpenAiConfig?.ApiKeys != null && appSetting.OpenAiConfig.ApiKeys!.Any())
    {
        var randomApiKey = appSetting.OpenAiConfig.ApiKeys[Random.Shared.Next(appSetting.OpenAiConfig.ApiKeys.Count)];
        httpClient.DefaultRequestHeaders.Add("Authorization", $"{Settings.Bearer} {randomApiKey}");
    }
});

builder.Services.AddScoped<IAmazonS3Service, AmazonS3Service>();
builder.Services.AddScoped<SpeechToTextPublisher>();
builder.Services.AddScoped<SpeechToTextAiPublisher>();
builder.Services.AddScoped<ResponseSpeechToTextPendingPublisher>();

builder.AddMassTransit(appSetting,
queues: new Dictionary<string, Type>
{
  { QueueSettings.StorageQueue.NameQueue.SpeechToTextAi, typeof(SpeechToTextAiConsumer) },
  { QueueSettings.LmsQueue.NameQueue.SpeechToTextPendingAi, typeof(SpeechToTextPendingAiConsumer) },
});

var app = builder.Build();
app.UseServices();
app.UseMiddleware<RequestTimeoutMiddleware>(TimeSpan.FromMinutes(5));
app.Run();

public class RequestTimeoutMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TimeSpan _timeout;

    public RequestTimeoutMiddleware(RequestDelegate next, TimeSpan timeout)
    {
        _next = next;
        _timeout = timeout;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var cts = new CancellationTokenSource(_timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted, cts.Token);

        var task = _next(context);
        var delayTask = Task.Delay(Timeout.InfiniteTimeSpan, linkedCts.Token);

        var completedTask = await Task.WhenAny(task, delayTask);
        if (completedTask == task)
        {
            await task;
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
            await context.Response.WriteAsync("Request timed out");
        }
    }
}

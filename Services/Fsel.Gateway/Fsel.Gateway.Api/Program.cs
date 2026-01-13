using Fsel.Common.ValueSettings;
using Fsel.Core.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Ocelot.DependencyInjection;
using Ocelot.Provider.Polly;

var builder = WebApplication.CreateBuilder(args);

var appSetting = builder.AddAppSettings<BaseAppSetting>();
builder.AddServices(appSetting);
builder.AddOpenIdAuthenticationJwtBearers(appSetting);

builder.Services.AddSwaggerForOcelot(builder.Configuration);
builder.Services.AddSwaggerGen();

builder.Configuration
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();
builder.Services.AddOcelot(builder.Configuration).AddPolly();

builder.WebHost.UseKestrel(options =>
{
    options.Limits.MaxRequestBodySize = long.MaxValue;
});
builder.WebHost.UseIIS();
builder.Services.Configure<IISServerOptions>(x =>
{
    x.MaxRequestBodyBufferSize = int.MaxValue;
    x.MaxRequestBodySize = long.MaxValue;
});
builder.Services.Configure<KestrelServerOptions>(x =>
{
    x.Limits.MaxRequestBodySize = long.MaxValue;
    x.Limits.MaxRequestBufferSize = long.MaxValue;
});
builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue;
    x.MultipartBoundaryLengthLimit = int.MaxValue;
    x.MultipartHeadersCountLimit = int.MaxValue;
    x.MultipartHeadersLengthLimit = int.MaxValue;
});

var app = builder.Build();
app.UseGatewayServices();
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

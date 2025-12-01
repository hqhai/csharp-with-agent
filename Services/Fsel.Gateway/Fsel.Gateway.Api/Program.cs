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
app.Run();

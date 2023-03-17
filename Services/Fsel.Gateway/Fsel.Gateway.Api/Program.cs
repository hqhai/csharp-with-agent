using Fsel.Common.ValueSettings;
using Fsel.Core.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

var appSetting = builder.AddAppSettings<BaseAppSetting>();
builder.AddServices();
builder.AddAuthenticationJwtBearers(appSetting);

builder.Services.AddSwaggerForOcelot(builder.Configuration);
builder.Services.AddSwaggerGen();

builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();
app.UseGatewayServices();

app.Run();

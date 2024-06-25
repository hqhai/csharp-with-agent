// Copyright (c) Atlantic. All rights reserved.

using Amazon.SimpleEmail;
using Fsel.Core.Extensions;
using Fsel.Sender.Application.Services;
using Fsel.Sender.Domain.ValueSettings;
using Fsel.Sender.Application.Services.SystemServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);

builder.Services.AddScoped<IAmazonSimpleEmailService, AmazonSimpleEmailServiceClient>();
builder.Services.AddScoped<SESWrapper>();
builder.AddRefitClients(typeof(ISystemService), appSetting?.Services?.SystemApiUrl);
var app = builder.Build();
app.UseServices();
app.Run();

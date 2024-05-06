// Copyright (c) Atlantic. All rights reserved.

using Amazon.SimpleEmail;
using Fsel.Core.Extensions;
using Fsel.Sender.Application.Services;
using Fsel.Sender.Domain.ValueSettings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddOpenIdSwaggerGens(appSetting);
builder.AddOpenIdAuthenticationJwtBearers(appSetting);

builder.Services.AddScoped<IAmazonSimpleEmailService, AmazonSimpleEmailServiceClient>();
builder.Services.AddScoped<SESWrapper>();
var app = builder.Build();
app.UseServices();
app.Run();

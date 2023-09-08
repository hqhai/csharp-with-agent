// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Extensions;
using Fsel.Storage.Application.Services.AmazonS3Services;
using Fsel.Storage.Infrastructure.ValueSettings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices();
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);

builder.Services.AddScoped<IAmazonS3Service, AmazonS3Service>();

var app = builder.Build();
app.UseServices();
app.Run();

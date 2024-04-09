// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Extensions;
using Fsel.Sender.Domain.ValueSettings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddOpenIdSwaggerGens(appSetting);
builder.AddOpenIdAuthenticationJwtBearers(appSetting);

var app = builder.Build();
app.UseServices();
app.Run();

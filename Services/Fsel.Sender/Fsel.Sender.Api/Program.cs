// Copyright (c) Atlantic. All rights reserved.

using Amazon.SimpleEmail;
using Fsel.Core.Extensions;
using Fsel.Sender.Application.Services;
using Fsel.Sender.Domain.ValueSettings;
using Fsel.Sender.Application.Services.SystemServices;
using Fsel.Sender.Domain.IRepositories;
using Fsel.Sender.Infrastructure.Repositories;
using Fsel.Sender.Infrastructure;
using Fsel.Sender.Application.Services.SMSServices.IRIS;
using Fsel.Sender.Application.Services.SMSServices.GAPIT;
using Refit;
using Fsel.Sender.Application.Services.ZaloServices;
using Fsel.Sender.Application.Services.UserServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddSwaggerGens(appSetting);
builder.AddDbContexts<SenderDBContext>();
builder.AddAuthenticationJwtBearers(appSetting);
builder.Services.AddScoped<IAmazonSimpleEmailService, AmazonSimpleEmailServiceClient>();
builder.Services.AddScoped<IMessageHistoryRepository, MessageHistoryRepository>();
builder.Services.AddScoped<IZaloService, ZaloService>();
builder.Services.AddScoped<SESWrapper>();
builder.AddRefitClients(typeof(ISystemService), appSetting?.Services?.SystemApiUrl);
builder.Services.AddRefitClient<IIRISServiceDC>().ConfigureHttpClient(c => c.BaseAddress = new Uri(appSetting?.Services?.IRISApiUrlDC ?? string.Empty));
builder.Services.AddRefitClient<IIRISServiceDR>().ConfigureHttpClient(c => c.BaseAddress = new Uri(appSetting?.Services?.IRISApiUrlDR ?? string.Empty));
builder.Services.AddRefitClient<IGAPITService>().ConfigureHttpClient(c => c.BaseAddress = new Uri(appSetting?.Services?.GAPITApiUrl ?? string.Empty));
builder.Services.AddRefitClient<IUserService>().ConfigureHttpClient(c => c.BaseAddress = new Uri(appSetting?.Services?.UserApiUrl ?? string.Empty));
var app = builder.Build();
app.UseServices();
app.Run();

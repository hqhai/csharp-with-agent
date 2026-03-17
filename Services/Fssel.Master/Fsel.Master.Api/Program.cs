// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Extensions;
using Fsel.Master.Domain.IRepositories;
using Fsel.Master.Infrastructure;
using Fsel.Master.Infrastructure.Repositories;
using Fsel.Master.Infrastructure.ValueSettings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddOpenIdSwaggerGens(appSetting);
builder.AddOpenIdAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<MasterDBContext, MasterReadDbContext>();

builder.Services.AddDataProtection();

builder.Services.AddScoped<IStudentProfileReportRepository, StudentProfileReportRepository>();

// Publisher
//builder.Services.AddScoped<CreateTokenHistoryPublisher>();

//Refit
//builder.AddRefitClients(typeof(IUserService), appSetting?.Services?.UserApiUrl);

//builder.AddRefitClients(typeof(IAppStoreService), appSetting?.Services?.AppStoreApiUrl);

builder.AddMassTransit(appSetting,
queues: new Dictionary<string, Type>
{
    //{ QueueSettings.OrderingQueue.NameQueue.NoticePayment, typeof(NoticePaymentConsumer) }
});
//builder.AddMassTransit(appSetting,
//queues: new Dictionary<string, Type>
//{
//    { QueueSettings.LmsQueue.NameQueue.OrderCreateNotification, typeof(CreateOrderConsumer) }
//});
var app = builder.Build();
app.UseServices();
app.Run();

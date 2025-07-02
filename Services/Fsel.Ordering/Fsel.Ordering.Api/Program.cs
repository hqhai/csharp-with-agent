// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Extensions;
using Fsel.Ordering.Application.Queues.Consumers;
using Fsel.Ordering.Application.Queues.Publishers;
using Fsel.Ordering.Application.Services.CourseService;
using Fsel.Ordering.Application.Services.InAppPurchase;
using Fsel.Ordering.Application.Services.InAppPurchase.Android;
using Fsel.Ordering.Application.Services.InAppPurchase.IOS;
using Fsel.Ordering.Application.Services.PayooService;
using Fsel.Ordering.Application.Services.SenderService;
using Fsel.Ordering.Application.Services.SystemService;
using Fsel.Ordering.Application.Services.TrainingService;
using Fsel.Ordering.Application.Services.UrBoxService;
using Fsel.Ordering.Application.Services.UserService;
using Fsel.Ordering.Domain.IRepositories;
using Fsel.Ordering.Infrastructure;
using Fsel.Ordering.Infrastructure.Common;
using Fsel.Ordering.Infrastructure.Repositories;
using Fsel.Ordering.Infrastructure.ValueSettings;
using Fsel.Shared.Constants;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddOpenIdSwaggerGens(appSetting);
builder.AddOpenIdAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<OrderingDbContext>();

builder.Services.AddDataProtection();

builder.Services.AddScoped<IPackageRepository, PackageRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
builder.Services.AddScoped<IUserReferralRepository, UserReferralRepository>();
builder.Services.AddScoped<IUserVoucherRepository, UserVoucherRepository>();
builder.Services.AddScoped<IOrderTransactionRepository, OrderTransactionRepository>();
builder.Services.AddScoped<INotificationProcessor, NotificationProcessor>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IPackageEventRepository, PackageEventRepository>();
builder.Services.AddScoped<IGooglePlayBillingService, GooglePlayBillingService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserVoucherLockRepository, UserVoucherLockRepository>();
builder.Services.AddScoped<VnPayLibrary>();

// Publisher
builder.Services.AddScoped<CreateTokenHistoryPublisher>();
builder.Services.AddScoped<NotificationMessagePublisher>();
builder.Services.AddScoped<AddExpiredDateForStudentPublisher>();
builder.Services.AddScoped<ChangeStatusOrderPublisher>();
builder.Services.AddScoped<AddFeatureMissionPublisher>();
builder.Services.AddScoped<AddCoinWhenCoursePurchasedPublisher>();

//Refit
builder.AddRefitClients(typeof(IUserService), appSetting?.Services?.UserApiUrl);
builder.AddRefitClients(typeof(ILmsCourseService), appSetting?.Services?.LmsCourseApiUrl);
builder.AddRefitClients(typeof(ITrainingService), appSetting?.Services?.ClassApiUrl);
builder.AddRefitClients(typeof(ISystemService), appSetting?.Services?.SystemApiUrl);
builder.AddRefitClients(typeof(IUrBoxService), appSetting?.Services?.UrBoxApiUrl);
builder.AddRefitClients(typeof(IPayooService), appSetting?.Services?.PayooApiUrl);
builder.AddRefitClients(typeof(ISenderServices), appSetting?.Services?.SenderApiUrl);
//builder.AddRefitClients(typeof(IAppStoreService), appSetting?.Services?.AppStoreApiUrl);
builder.Services.AddRefitClient<IAppStoreService>().ConfigureHttpClient(x =>
{
    x.BaseAddress = new Uri(appSetting?.Services?.AppStoreApiUrl ?? string.Empty);
});

builder.AddMassTransit(appSetting,
queues: new Dictionary<string, Type>
{
    { QueueSettings.OrderingQueue.NameQueue.NoticePayment, typeof(NoticePaymentConsumer) },
    { QueueSettings.OrderingQueue.NameQueue.JobActiveEvent, typeof(JobActiveEventConsumer) },
});
//builder.AddMassTransit(appSetting,
//queues: new Dictionary<string, Type>
//{
//    { QueueSettings.LmsQueue.NameQueue.OrderCreateNotification, typeof(CreateOrderConsumer) }
//});
var app = builder.Build();
app.UseServices();
app.Run();

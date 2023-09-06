// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Extensions;
using Fsel.Ordering.Application.Queues.Publishers;
using Fsel.Ordering.Application.Services.CourseService;
using Fsel.Ordering.Application.Services.SystemService;
using Fsel.Ordering.Application.Services.TrainingService;
using Fsel.Ordering.Application.Services.UserService;
using Fsel.Ordering.Domain.IRepositories;
using Fsel.Ordering.Infrastructure;
using Fsel.Ordering.Infrastructure.Repositories;
using Fsel.Ordering.Infrastructure.ValueSettings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices();
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<OrderingDbContext>();

builder.Services.AddScoped<IPackageRepository, PackageRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
builder.Services.AddScoped<IUserReferralRepository, UserReferralRepository>();
builder.Services.AddScoped<IUserVoucherRepository, UserVoucherRepository>();
builder.Services.AddScoped<NotificationMessagePublisher>();

builder.AddRefitClients(typeof(IUserService), appSetting?.Services?.UserApiUrl);
builder.AddRefitClients(typeof(ILmsCourseService), appSetting?.Services?.LmsCourseApiUrl);
builder.AddRefitClients(typeof(ITrainingService), appSetting?.Services?.ClassApiUrl);
builder.AddRefitClients(typeof(ISystemService), appSetting?.Services?.SystemApiUrl);

builder.AddMassTransit(appSetting);
var app = builder.Build();
app.UseServices();
app.Run();

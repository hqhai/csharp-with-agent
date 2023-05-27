// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Extensions;
using Fsel.Ordering.Application.Services.CourseService;
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

builder.AddRefitClients(typeof(ICourseService), appSetting?.Services?.CourseApiUrl);
var app = builder.Build();

app.UseServices();
app.Run();

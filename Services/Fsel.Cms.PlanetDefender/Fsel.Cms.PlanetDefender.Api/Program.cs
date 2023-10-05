// Copyright (c) Atlantic. All rights reserved.

using Fsel.Cms.PlanetDefender.Domain.IRepositories;
using Fsel.Cms.PlanetDefender.Infrastructure;
using Fsel.Cms.PlanetDefender.Infrastructure.Repositories;
using Fsel.Cms.PlanetDefender.Infrastructure.ValueSettings;
using Fsel.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<CmsPlanetDefenderDbContext>();

builder.Services.AddScoped<IStudentGameInfoRepository, StudentGameInfoRepository>();

var app = builder.Build();
app.UseServices();
app.Run();

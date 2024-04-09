// Copyright (c) Atlantic. All rights reserved.

using Fsel.Cms.PlanetDefender.Application.Queues;
using Fsel.Cms.PlanetDefender.Application.Services.LmsCourseServices;
using Fsel.Cms.PlanetDefender.Application.Services.SystemServices;
using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
using Fsel.Cms.PlanetDefender.Domain.IRepositories;
using Fsel.Cms.PlanetDefender.Infrastructure;
using Fsel.Cms.PlanetDefender.Infrastructure.Repositories;
using Fsel.Cms.PlanetDefender.Infrastructure.ValueSettings;
using Fsel.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddOpenIdSwaggerGens(appSetting);
builder.AddOpenIdAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<CmsPlanetDefenderDbContext>();

builder.Services.AddScoped<IStudentGameInfoRepository, StudentGameInfoRepository>();
builder.Services.AddScoped<IZMatterRepository, ZMatterRepository>();
builder.Services.AddScoped<IGameplayTimeConfigRepository, GameplayTimeConfigRepository>();
builder.Services.AddScoped<IGameplayRuleConfigRepository, GameplayRuleConfigRepository>();
builder.Services.AddScoped<IWheelOfBuffRepository, WheelOfBuffRepository>();
builder.Services.AddScoped<ISpaceShipRepository, SpaceShipRepository>();
builder.Services.AddScoped<IGameHistoryRepository, GameHistoryRepository>();
builder.Services.AddScoped<IAvatarImageRepository, AvatarImageRepository>();
builder.Services.AddScoped<IStudentSpaceShipRepository, StudentSpaceShipRepository>();
builder.Services.AddScoped<IStudentTagNameRepository, StudentTagNameRepository>();
builder.Services.AddScoped<IGameAnswerRepository, GameAnswerRepository>();
builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();

builder.AddRefitClients(typeof(ISystemService), appSetting?.Services?.SystemApiUrl);
builder.AddRefitClients(typeof(IUserService), appSetting?.Services?.UserApiUrl);
builder.AddRefitClients(typeof(ICourseService), appSetting?.Services?.CourseApiUrl);

builder.Services.AddScoped<DeleteGuestStudentPublisher>();
builder.AddMassTransit(appSetting);
var app = builder.Build();
app.UseServices();
app.Run();

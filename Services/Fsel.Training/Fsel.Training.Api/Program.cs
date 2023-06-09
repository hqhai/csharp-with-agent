// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ValueSettings;
using Fsel.Core.Extensions;
using Fsel.Training.Application.Services.CourseServices;
using Fsel.Training.Application.Services.UserServices;
using Fsel.Training.Domain.IRepositories;
using Fsel.Training.Infrastructure;
using Fsel.Training.Infrastructure.Repositories;
using Fsel.Training.Infrastructure.ValueSettings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices();
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<TrainingDbContext>();
builder.Services.AddScoped<IClassRepository, ClassRepository>();
builder.Services.AddScoped<IClassStudentRepository, ClassStudentRepository>();
builder.AddRefitClients(typeof(IUserService), appSetting?.Services?.UserApiUrl);
builder.AddRefitClients(typeof(ICourseService), appSetting?.Services?.CourseApiUrl);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseServices();
app.Run();

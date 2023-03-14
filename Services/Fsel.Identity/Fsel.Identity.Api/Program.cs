using Fsel.Core.Extensions;
using Fsel.Identity.Application.Services;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Infrastructure;
using Fsel.Identity.Infrastructure.Repositories;
using Fsel.Identity.Infrastructure.ValueSettings;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices();
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<UserDbContext>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IHumanRepository, HumanRepository>();
builder.Services.AddScoped<IParentRepository, ParentRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IParentStudentRepository, ParentStudentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.AddRefitClients(typeof(ISenderService), appSetting?.Services?.SenderApiUrl);

builder.Services.AddIdentity<User, IdentityRole>()
        .AddEntityFrameworkStores<UserDbContext>()
        .AddDefaultTokenProviders();

var app = builder.Build();

app.UseServices();
app.Run();

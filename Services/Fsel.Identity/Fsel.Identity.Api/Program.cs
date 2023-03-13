using Fsel.Core.Extensions;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Infrastructure;
using Fsel.Identity.Infrastructure.ValueSettings;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices();
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<UserDbContext>();

builder.Services.AddIdentity<User, Role>()
        .AddEntityFrameworkStores<UserDbContext>()
        .AddDefaultTokenProviders();

var app = builder.Build();

app.UseServices();
app.Run();

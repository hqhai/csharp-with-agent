using Fsel.Common.ConfigSettings;
using Fsel.Common.Constants;
using Fsel.Core.Extensions;
using Fsel.Identity.Application.Services;
using Fsel.Identity.Common.ConfigSettings;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Infrastructure;
using Fsel.Identity.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Refit;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices();
builder.AddSwaggerGens(appSetting);
builder.AddDbContexts<UserDbContext>();
builder.AddAuthentication();

builder.Services.AddIdentity<User, IdentityRole>()
        .AddEntityFrameworkStores<UserDbContext>()
        .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.AddRefitClients(typeof(ISenderService), appSetting?.Services?.SenderApiUrl);

//App config
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//app configurations

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
using Fsel.Common.Constants;
using Fsel.Identity.Application.Services;
using Fsel.Identity.Common.ConfigSettings;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Infrastructure;
using Fsel.Identity.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Refit;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var appDomainAssembly = AppDomain.CurrentDomain.GetAssemblies();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Authentication", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Jwt Authorization",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference =new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },new string[] {}
        }
    });
});

builder.Services.AddApiVersioning();
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services
    .AddMediatR(AppDomain.CurrentDomain.GetAssemblies())
    .AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies())
    .AddHttpContextAccessor();

var appSetting = builder.Configuration.Get<AppSetting>() ?? new AppSetting();
builder.Services.AddSingleton(appSetting);

builder.Services.AddDbContext<UserDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString(Settings.DefaultConnection)));
builder.Services.AddIdentity<User, IdentityRole>()
        .AddEntityFrameworkStores<UserDbContext>()
        .AddDefaultTokenProviders();

builder.Services.AddAuthentication();
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    //options.Password.RequireNonAlphanumeric = true;
    //options.Password.RequireUppercase = false;
    //options.Password.RequireLowercase = false;
    //options.Password.RequiredUniqueChars = 6;
    options.SignIn.RequireConfirmedEmail = true;
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@.";
});

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddRefitClient<ISenderService>().ConfigureHttpClient(x =>
{
    x.BaseAddress = new Uri("https://localhost:7036/api/v1");
});

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
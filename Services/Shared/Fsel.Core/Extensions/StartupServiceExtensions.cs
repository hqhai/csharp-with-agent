using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Fsel.Common.Constants;
using Fsel.Common.ValueSettings;
using Fsel.Core.Base;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Refit;

namespace Fsel.Core.Extensions
{
    public static class StartupServiceExtensions
    {
        public static void AddServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddEndpointsApiExplorer();
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
            builder.AddAuthContexts();
        }

        private static void AddAuthContexts(this WebApplicationBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddScoped(x =>
            {
                var authContext = new AuthContext();
                var httpContextAccessor = x.GetService<IHttpContextAccessor>();
                var httpContext = httpContextAccessor?.HttpContext;
                var user = httpContextAccessor?.HttpContext?.User;
                if (user != null)
                {
                    if (Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out Guid id))
                    {
                        authContext.CurrentUserId = id;
                    }
                    authContext.CurrentUsername = user.FindFirstValue(ClaimTypes.Name);
                    authContext.CurrentFullName = user.FindFirstValue(ClaimTypes.GivenName);
                    authContext.Email = user.FindFirstValue(ClaimTypes.Email);
                }
                return authContext;
            });
        }

        public static void AddAuthentication(this WebApplicationBuilder builder)
        {
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
        }

        public static void AddAuthenticationJwtBearers(this WebApplicationBuilder builder, BaseAppSetting? baseAppSetting)
        {
            if (baseAppSetting != null)
            {
                builder.Services
                .AddAuthentication(option =>
                {
                    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(option =>
                {
                    option.SaveToken = true;
                    option.RequireHttpsMetadata = false;
                    option.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidAudience = baseAppSetting?.Jwt?.Audience,
                        ValidIssuer = baseAppSetting?.Jwt?.Issuer,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(baseAppSetting?.Jwt?.SecretKey ?? string.Empty))
                    };
                });
            }
        }

        public static T? AddAppSettings<T>(this WebApplicationBuilder builder) where T : BaseAppSetting
        {
            var _baseAppSetting = builder.Configuration.Get<T>();
            if (_baseAppSetting != null)
                builder.Services.AddSingleton<T>(_baseAppSetting);
            return _baseAppSetting;
        }

        public static void AddDbContexts<TContext>(this WebApplicationBuilder builder) where TContext : DbContext
        {
            builder.Services.AddDbContext<TContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString(Settings.DefaultConnection)));
        }

        public static void AddRefitClients(this WebApplicationBuilder builder, Type refitInterfaceType, string? url)
        {
            builder.Services.AddRefitClient(refitInterfaceType).ConfigureHttpClient(x =>
            {
                x.BaseAddress = new Uri(url ?? string.Empty);
            }).AddHttpMessageHandler<AuthorizationMessageHandler>();
        }

        public static void AddSwaggerGens(this WebApplicationBuilder builder, BaseAppSetting? baseAppSetting)
        {
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = baseAppSetting?.ServiceName, Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = $"Jwt {baseAppSetting?.ServiceName}",
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
        }
    }
}

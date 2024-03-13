// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using  Fsel.Identity.Infrastructure.Configs;
using Fsel.Common.Constants;
using Fsel.Core.Base;
using Fsel.Core.Extensions;
using Fsel.Identity.Application.Queues.Publishers;
using Fsel.Identity.Application.Services.InteractionService;
using Fsel.Identity.Application.Services.LmsCourseService;
using Fsel.Identity.Application.Services.OrderService;
using Fsel.Identity.Application.Services.SenderService;
using Fsel.Identity.Application.Services.SystemService;
using Fsel.Identity.Application.Services.TrainingService;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Infrastructure;
using Fsel.Identity.Infrastructure.Providers;
using Fsel.Identity.Infrastructure.Repositories;
using Fsel.Identity.Infrastructure.ValueSettings;
using IdentityServer4;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Fsel.Authentication.Infrastructure.Configs;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(UserDbContext).Assembly.GetName().Name;
var defaultConnString = builder.Configuration.GetConnectionString(Settings.DefaultConnection);
var appSetting = builder.AddAppSettings<AppSetting>();

builder.AddServices(appSetting);
//builder.AddSwaggerGens(appSetting);
//builder.AddAuthenticationJwtBearers(appSetting);
builder.AddAuthenticationIdentity();
builder.AddDbContexts<UserDbContext>();
builder.Services.AddDataProtection().PersistKeysToDbContext<UserDbContext>();
//.DisableAutomaticKeyGeneration();
builder.Services.AddAntiforgery();

builder.AddIdentity<User, Role, UserDbContext>().AddTotpProvider();

builder.Services.AddIdentityServer(options =>
{
    options.Authentication.CookieSameSiteMode = SameSiteMode.None;
})
.AddInMemoryApiScopes(Config.ApiScopes)
.AddInMemoryIdentityResources(Config.IdentityResources)
.AddInMemoryApiResources(Config.ApiResources)
.AddInMemoryClients(Config.Clients)
.AddAspNetIdentity<User>()
.AddConfigurationStore(options => options.ConfigureDbContext = b => b.UseSqlServer(defaultConnString, opt => opt.MigrationsAssembly(assembly)))
.AddConfigurationStoreCache()
.AddOperationalStore(options => options.ConfigureDbContext = b => b.UseSqlServer(defaultConnString, opt => opt.MigrationsAssembly(assembly)))
.AddDeveloperSigningCredential();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = appSetting?.Authentication?.Google?.ClientId ?? string.Empty;
        options.ClientSecret = appSetting?.Authentication?.Google?.ClientSecret ?? string.Empty;
        options.Events = new OAuthEvents
        {
            OnRedirectToAuthorizationEndpoint = context =>
            {
                if (appSetting?.Authentication?.Google?.RedirectUriParams != null)
                {
                    appSetting?.Authentication?.Google?.RedirectUriParams.ForEach(param =>
                    {
                        context.Response.Redirect(context.RedirectUri + param);
                    });
                }

                return Task.CompletedTask;
            }
        };
    })
    .AddFacebook(facebookOptions =>
    {
        facebookOptions.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
        facebookOptions.AppId = appSetting?.Authentication?.Facebook?.ClientId ?? string.Empty;
        facebookOptions.AppSecret = appSetting?.Authentication?.Facebook?.ClientId ?? string.Empty;
        facebookOptions.CallbackPath = appSetting?.Authentication?.Facebook?.Callback ?? string.Empty;
    })
    .AddCookie(options =>
    {
        options.CookieManager = new ChunkingCookieManager();
        options.Cookie.IsEssential = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

builder.Services.ConfigureExternalCookie(options =>
{
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});


builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserTokenRepository, UserTokenRepository>();
builder.Services.AddScoped<IHumanRepository, HumanRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<IParentRepository, ParentRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IUserOtpRepository, UserOtpRepository>();
builder.Services.AddScoped<IParentStudentRepository, ParentStudentRepository>();
builder.Services.AddScoped<ICSORepository, CSORepository>();
builder.Services.AddScoped<ITeacherBankAccountRepository, TeacherBankAccountRepository>();
builder.Services.AddScoped<IUserSettingRepository, UserSettingRepository>();
builder.Services.AddScoped<IPlatformRepository, PlatformRepository>();
builder.Services.AddScoped<IUserPlatformRepository, UserPlatformRepository>();
builder.Services.AddScoped<IStudentDailyStreakRepository, StudentDailyStreakRepository>();
builder.Services.AddScoped<IStudentRankingRepository, StudentRankingRepository>();
builder.Services.AddScoped<IStudentTrialRegistrationRepository, StudentTrialRegistrationRepository>();


// Queue
builder.Services.AddScoped<LeaderBoardPublisher>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IStudentFocusTimeRepository, StudentFocusTimeRepository>();
builder.Services.AddScoped<QuestBoardPublisher>();


builder.AddRefitClients(typeof(ISenderService), appSetting?.Services?.SenderApiUrl);
builder.AddRefitClients(typeof(IOrderService), appSetting?.Services?.OrderApiUrl);
builder.AddRefitClients(typeof(IInteractionService), appSetting?.Services?.InteractionApiUrl);
builder.AddRefitClients(typeof(ITrainingService), appSetting?.Services?.ClassApiUrl);
builder.AddRefitClients(typeof(ILmsCourseService), appSetting?.Services?.LmsCourseApiUrl);
builder.AddRefitClients(typeof(ISystemService), appSetting?.Services?.SystemApiUrl);

builder.AddMassTransit(appSetting);


builder.Services.AddMvc();
builder.Services.AddMvcCore();
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddLocalApiAuthentication();

//App config
var app = builder.Build();
app.UseStaticFiles();
app.UseIdentityServer();
app.UseAuthentication();
app.MapControllers();
app.MapDefaultControllerRoute();
app.UseHttpsRedirection();
app.UseCookiePolicy(new CookiePolicyOptions
{
    // HttpOnly =  HttpOnlyPolicy.Always,
    MinimumSameSitePolicy = SameSiteMode.None,
    Secure = CookieSecurePolicy.Always
    // MinimumSameSitePolicy = SameSiteMode.Lax
});
//app.UseServices();

#region Initialized Database
using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()!.CreateScope())
{
    serviceScope.ServiceProvider.GetRequiredService<UserDbContext>().Database.Migrate();
    serviceScope.ServiceProvider.GetRequiredService<IdentityServer4.EntityFramework.DbContexts.PersistedGrantDbContext>().Database.Migrate();

    var context = serviceScope.ServiceProvider.GetRequiredService<IdentityServer4.EntityFramework.DbContexts.ConfigurationDbContext>();
    context.Database.Migrate();

    var mapperClient = new MapperConfiguration(cfg => cfg.AddProfile<ClientMapperProfile>()).CreateMapper();
    var mapperIdentityResource = new MapperConfiguration(cfg => cfg.AddProfile<IdentityResourceMapperProfile>()).CreateMapper();
    var mapperApiScope = new MapperConfiguration(cfg => cfg.AddProfile<ScopeMapperProfile>()).CreateMapper();
    var mapperApiResource = new MapperConfiguration(cfg => cfg.AddProfile<ApiResourceMapperProfile>()).CreateMapper();

    foreach (var client in Config.Clients)
    {
        var clientDB = await context.Clients.FirstOrDefaultAsync(x => x.ClientId == client.ClientId);
        if (clientDB == null)
        {
            context.Clients.Add(client.ToEntity());
        }
    }

    foreach (var resource in Config.IdentityResources)
    {
        var resourceDB = await context.IdentityResources.FirstOrDefaultAsync(x => x.Name == resource.Name);
        if (resourceDB == null)
        {
            context.IdentityResources.Add(resource.ToEntity());
        }
    }

    foreach (var apiScope in Config.ApiScopes)
    {
        var apiScopeDB = await context.ApiScopes.FirstOrDefaultAsync(x => x.Name == apiScope.Name);
        if (apiScopeDB == null)
        {
            context.ApiScopes.Add(apiScope.ToEntity());
        }
    }

    foreach (var apiResource in Config.ApiResources)
    {
        var apiResourceDB = await context.ApiResources.FirstOrDefaultAsync(x => x.Name == apiResource.Name);
        if (apiResourceDB == null)
        {
            context.ApiResources.Add(apiResource.ToEntity());
        }
    }

    context.SaveChanges();
}
#endregion

await app.RunAsync();

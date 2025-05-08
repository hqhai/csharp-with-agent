// Copyright (c) Atlantic. All rights reserved.

using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using AutoMapper;
using Fsel.Authentication.Infrastructure.Configs;
using Fsel.Common.Constants;
using Fsel.Core.Extensions;
using Fsel.Identity.Application.Events;
using Fsel.Identity.Application.Queues.Publishers;
using Fsel.Identity.Application.Services.InteractionService;
using Fsel.Identity.Application.Services.LmsCourseService;
using Fsel.Identity.Application.Services.OrderService;
using Fsel.Identity.Application.Services.SenderService;
using Fsel.Identity.Application.Services.SystemService;
using Fsel.Identity.Application.Services.TrainingService;
using Fsel.Identity.Application.Services.UserProfileService;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Infrastructure;
using Fsel.Identity.Infrastructure.Providers;
using Fsel.Identity.Infrastructure.Repositories;
using Fsel.Identity.Infrastructure.ValueSettings;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(UserDbContext).Assembly.GetName().Name;
var defaultConnString = builder.Configuration.GetConnectionString(Settings.DefaultConnection);
var appSetting = builder.AddAppSettings<AppSetting>();

builder.AddServices(appSetting);
//builder.AddSwaggerGens(appSetting);
//builder.AddAuthenticationJwtBearers(appSetting);
builder.AddConfigureIdentityOptions();
builder.AddDbContexts<UserDbContext>();
builder.Services.AddDataProtection().PersistKeysToDbContext<UserDbContext>();
//.DisableAutomaticKeyGeneration();
builder.Services.AddAntiforgery();

builder.AddIdentity<User, Role, UserDbContext>().AddTotpProvider();
builder.Services.AddIdentityServer(options =>
{
    options.Authentication.CookieSameSiteMode = SameSiteMode.None;
    options.EmitStaticAudienceClaim = false;
    options.Events.RaiseSuccessEvents = true;
})
.AddInMemoryApiScopes(Config.ApiScopes)
.AddInMemoryIdentityResources(Config.IdentityResources)
.AddInMemoryApiResources(Config.ApiResources)
.AddInMemoryClients(Config.Clients)
.AddAspNetIdentity<User>()
.AddConfigurationStore(options => options.ConfigureDbContext = b => b.UseSqlServer(defaultConnString, opt => opt.MigrationsAssembly(assembly)))
.AddConfigurationStoreCache()
.AddOperationalStore(options =>
{
    options.ConfigureDbContext = b => b.UseSqlServer(defaultConnString, opt => opt.MigrationsAssembly(assembly));
    options.EnableTokenCleanup = true;
    options.TokenCleanupInterval = 3600;
})
.AddDeveloperSigningCredential()
.AddProfileService<UserProfileService>();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.UsePkce = true;
        //options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
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

        //options.Scope.Add("https://www.googleapis.com/auth/user.phonenumbers.read");
        //options.Scope.Add("https://www.googleapis.com/auth/plus.me");
        //options.Scope.Add("https://www.googleapis.com/auth/userinfo.email");
        //options.Scope.Add("https://www.googleapis.com/auth/userinfo.profile");
        //options.Scope.Add("gender");
        //options.Scope.Add("phone");
    })
    .AddFacebook(facebookOptions =>
    {
        facebookOptions.UsePkce = true;
        //facebookOptions.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
        facebookOptions.AppId = appSetting?.Authentication?.Facebook?.ClientId ?? string.Empty;
        facebookOptions.AppSecret = appSetting?.Authentication?.Facebook?.ClientSecret ?? string.Empty;
        facebookOptions.CallbackPath = appSetting?.Authentication?.Facebook?.Callback ?? string.Empty;
        facebookOptions.SendAppSecretProof = true;
        facebookOptions.Fields.Add("id");
        facebookOptions.Fields.Add("email");
        facebookOptions.Fields.Add("name");
        facebookOptions.Fields.Add("birthday");
        facebookOptions.Fields.Add("gender");
        //facebookOptions.Fields.Add("picture");
        //facebookOptions.Fields.Add("public_profile");
        //facebookOptions.Fields.Add("phone");
        //facebookOptions.UserInformationEndpoint = "https://graph.facebook.com/v2.8/me?fields=id,name,email,birthday,gender,phone,avatar_2d_profile_picture";
    })
    .AddOAuth<OAuthOptions, ZaloOAuthHandler>("Zalo", options =>
    {
        options.UsePkce = true;
        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.ClientId = "3677545940964641090";
        options.ClientSecret = "vqMb7BGNKESCNzTWU389";
        options.CallbackPath = "/signin-zalo";
        options.AuthorizationEndpoint = "https://oauth.zaloapp.com/v4/permission";
        options.TokenEndpoint = "https://oauth.zaloapp.com/v4/access_token";
        options.UserInformationEndpoint = "https://graph.zalo.me/v2.0/me?fields=id,name,picture,birthday,gender,phone,email";
        options.SaveTokens = true;

        options.Scope.Add("profile");
        options.Scope.Add("id");
        options.Scope.Add("name");

        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        options.ClaimActions.MapJsonKey(ClaimTypes.MobilePhone, "phone");
        options.ClaimActions.MapJsonKey(ClaimTypes.Gender, "gender");
        options.ClaimActions.MapJsonKey(ClaimTypes.DateOfBirth, "birthday");

        options.Events = new OAuthEvents
        {
            OnAccessDenied = context =>
            {
                var logger = context.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("OAuthEvents");
                logger.LogError("OnAccessDenied_Cookies: {cookies}", context.Request.Headers["Cookie"].ToString());
                return Task.CompletedTask;
            },
            OnTicketReceived = context =>
            {
                var logger = context.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("OAuthEvents");
                logger.LogError("OnTicketReceived_Cookies: {cookies}", context.Request.Headers["Cookie"].ToString());
                return Task.CompletedTask;
            },
            OnRemoteFailure = context =>
            {
                var logger = context.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("OAuthEvents");
                logger.LogError("OnRemoteFailure_Cookies: {cookies}", context.Request.Headers["Cookie"].ToString());
                return Task.CompletedTask;
            },
            OnCreatingTicket = async context =>
            {
                var logger = context.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("OAuthEvents");
                logger.LogError("OnCreatingTicket_Cookies: {cookies}", context.Request.Headers["Cookie"].ToString());

                var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                response.EnsureSuccessStatusCode();

                using var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                logger?.LogError("OnCreatingTicket_AccessToken: {json}", context.AccessToken);
                logger?.LogError("OnCreatingTicket_Zalo_UserInfo: {json}", user.RootElement.ToString());
                context.RunClaimActions(user.RootElement);
            },
            OnRedirectToAuthorizationEndpoint = context =>
            {
                var logger = context.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("OAuthEvents");
                logger.LogError("OnRedirectToAuthorizationEndpoint_Cookies: {cookies}", context.Request.Headers["Cookie"].ToString());

                var uri = new UriBuilder(context.RedirectUri);
                var query = QueryHelpers.ParseQuery(uri.Query);

                // Đổi tên client_id => app_id
                if (query.ContainsKey("client_id"))
                {
                    var appId = query["client_id"];
                    query.Remove("client_id");
                    query["app_id"] = appId;
                }

                // Gán lại query string đã chỉnh sửa
                uri.Query = new QueryBuilder(query.SelectMany(kvp => kvp.Value, (kvp, v) => new KeyValuePair<string, string>(kvp.Key, v))).ToQueryString().ToString();

                context.Response.Redirect(uri.ToString());
                return Task.CompletedTask;
            }
        };
    })
    //.AddOpenIdConnect("oidc", "Zalo", options =>
    //{
    //    options.Authority = "https://oauth.zaloapp.com/v4/permission";
    //    options.ClientId = "implicit";

    //    options.TokenValidationParameters = new TokenValidationParameters
    //    {
    //        NameClaimType = "name",
    //        RoleClaimType = "role"
    //    };
    //})
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

builder.Services.AddSingleton<ICorsPolicyService>((container) =>
{
    var logger = container.GetRequiredService<ILogger<DefaultCorsPolicyService>>();
    return new DefaultCorsPolicyService(logger)
    {
        AllowAll = true
        //AllowedOrigins = { "https://localhost:4400", "https://localhost:7088" },
    };
});

builder.WebHost.UseKestrel();
//builder.WebHost.UseFacebookAuthentication();
//builder.WebHost.UseKestrel(options =>
//{
//    options.Listen(IPAddress.Loopback, 443, listenOptions =>
//    {
//        listenOptions.UseHttps("certificate.pfx", "password");
//    });
//});

var fordwardedHeaderOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    RequireHeaderSymmetry = false
};
fordwardedHeaderOptions.KnownNetworks.Clear();
fordwardedHeaderOptions.KnownProxies.Clear();
builder.Services.Configure<ForwardedHeadersOptions>(x => x = fordwardedHeaderOptions);

//Repository
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserTokenRepository, UserTokenRepository>();
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
builder.Services.AddScoped<IUserCourseSettingRepository, UserCourseSettingRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IStudentFocusTimeRepository, StudentFocusTimeRepository>();
builder.Services.AddScoped<IStudentCompetitionSnapShotRepository, StudentCompetitionSnapShotRepository>();
builder.Services.AddScoped<ICompetitionEventsRepository, CompetitionEventsRepository>();
builder.Services.AddScoped<IEventRegistrationRepository, EventRegistrationRepository>();
builder.Services.AddScoped<IStudentCompetitionEventsRepository, StudentCompetitionEventRepository>();
builder.Services.AddScoped<IStudentRankingEventRepository, StudentRankingEventRepository>();
builder.Services.AddScoped<IUserReferralRepository, UserReferralRepository>();
builder.Services.AddScoped<IEventRegistrationRepository, EventRegistrationRepository>();
builder.Services.AddScoped<IUserDeletionRepository, UserDeletionRepository>();

// Event
builder.Services.AddTransient<IEventSink, TokenIssuedEventHandler>();

// Queue
builder.Services.AddScoped<LeaderBoardPublisher>();
builder.Services.AddScoped<QuestBoardPublisher>();
builder.Services.AddScoped<NotificationMessagePublisher>();
builder.Services.AddScoped<CreateTokenHistoryPublisher>();

//Refit
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
builder.Services.AddHttpsRedirection(opt => opt.HttpsPort = 443);

//App config
var app = builder.Build();
app.UseLanguages();
app.UseStaticFiles();
app.UseIdentityServer();
app.UseCertificateForwarding();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultControllerRoute();
app.UseHttpsRedirection();
app.UseCookiePolicy(new CookiePolicyOptions
{
    // HttpOnly =  HttpOnlyPolicy.Always,
    MinimumSameSitePolicy = SameSiteMode.None,
    Secure = CookieSecurePolicy.Always
});
//app.Use(async (context, next) =>
//{
//    //context.SetIdentityServerOrigin("https://fsel-auth-testing.fsel.edu.vn");
//    //context.Request.Scheme = "https";
//    //context.Request.IsHttps = true;
//    await next();
//});

app.UseCors();
app.UseCors(Settings.CorsPolicy);

app.UseForwardedHeaders(fordwardedHeaderOptions);
app.UseDefaultServices();

//app.UseServices();

#region Initialized Database

using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()!.CreateScope())
{
    //serviceScope.ServiceProvider.GetRequiredService<UserDbContext>().Database.Migrate();
    serviceScope.ServiceProvider.GetRequiredService<IdentityServer4.EntityFramework.DbContexts.PersistedGrantDbContext>().Database.Migrate();

    var context = serviceScope.ServiceProvider.GetRequiredService<IdentityServer4.EntityFramework.DbContexts.ConfigurationDbContext>();
    context.Database.Migrate();

    var mapperClient = new MapperConfiguration(cfg => cfg.AddProfile<ClientMapperProfile>()).CreateMapper();
    var mapperIdentityResource = new MapperConfiguration(cfg => cfg.AddProfile<IdentityResourceMapperProfile>()).CreateMapper();
    var mapperApiScope = new MapperConfiguration(cfg => cfg.AddProfile<ScopeMapperProfile>()).CreateMapper();
    var mapperApiResource = new MapperConfiguration(cfg => cfg.AddProfile<ApiResourceMapperProfile>()).CreateMapper();

    foreach (var client in Config.Clients)
    {
        var clientDB = await context.Clients
                        .Include(x => x.RedirectUris)
                        .Include(x => x.PostLogoutRedirectUris)
                        .Include(x => x.ClientSecrets)
                        .Include(x => x.Claims)
                        .Include(x => x.AllowedScopes)
                        .Include(x => x.AllowedCorsOrigins)
                        .Include(x => x.AllowedGrantTypes)
                        .Include(x => x.Properties)
                        .Include(x => x.IdentityProviderRestrictions)
                        .Where(c => c.ClientId == client.ClientId)
                        .FirstOrDefaultAsync(x => x.ClientId == client.ClientId);
        if (clientDB != null)
        {
            context.Clients.Remove(clientDB);
        }
        context.Clients.Add(client.ToEntity());
    }

    foreach (var resource in Config.IdentityResources)
    {
        var resourceDB = await context.IdentityResources.FirstOrDefaultAsync(x => x.Name == resource.Name);
        if (resourceDB != null)
        {
            context.IdentityResources.Remove(resourceDB);
        }
        context.IdentityResources.Add(resource.ToEntity());
    }

    foreach (var apiScope in Config.ApiScopes)
    {
        var apiScopeDB = await context.ApiScopes.FirstOrDefaultAsync(x => x.Name == apiScope.Name);
        if (apiScopeDB != null)
        {
            context.ApiScopes.Remove(apiScopeDB);
        }
        context.ApiScopes.Add(apiScope.ToEntity());
    }

    foreach (var apiResource in Config.ApiResources)
    {
        var apiResourceDB = await context.ApiResources.FirstOrDefaultAsync(x => x.Name == apiResource.Name);
        if (apiResourceDB != null)
        {
            context.ApiResources.Remove(apiResourceDB);
        }
        context.ApiResources.Add(apiResource.ToEntity());
    }

    context.SaveChanges();
}

#endregion Initialized Database

await app.RunAsync();

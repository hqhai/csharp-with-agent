// Copyright (c) Atlantic. All rights reserved.

using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AutoMapper;
using Fsel.Authentication.Infrastructure.Configs;
using Fsel.Common.Constants;
using Fsel.Common.Helpers;
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
using Fsel.Identity.Infrastructure.Common;
using Fsel.Identity.Infrastructure.Providers;
using Fsel.Identity.Infrastructure.Repositories;
using Fsel.Identity.Infrastructure.ValueSettings;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using static IdentityServer4.IdentityServerConstants;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(UserDbContext).Assembly.GetName().Name;
var tenantMasterConnection = builder.Configuration.GetConnectionString(Settings.TenantMasterConnection);
var appSetting = builder.AddAppSettings<AppSetting>();

builder.AddServices(appSetting);
builder.AddDbContexts<UserDbContext>();

#region AddOpenIdServices

builder.AddConfigureIdentityOptions();
builder.Services.AddDataProtection().PersistKeysToDbContext<UserDbContext>();
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
.AddConfigurationStore(options => options.ConfigureDbContext = b => b.UseSqlServer(tenantMasterConnection, opt => opt.MigrationsAssembly(assembly)))
.AddConfigurationStoreCache()
.AddOperationalStore(options =>
{
    options.ConfigureDbContext = b => b.UseSqlServer(tenantMasterConnection, opt => opt.MigrationsAssembly(assembly));
    options.EnableTokenCleanup = true;
    options.TokenCleanupInterval = 3600;
})
.AddDeveloperSigningCredential()
.AddProfileService<UserProfileService>();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.UsePkce = true;
        options.ClientId = appSetting?.Authentication?.Google?.ClientId ?? string.Empty;
        options.ClientSecret = appSetting?.Authentication?.Google?.ClientSecret ?? string.Empty;
        options.CallbackPath = appSetting?.Authentication?.Google?.Callback ?? string.Empty;
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
            },
            OnRemoteFailure = context =>
            {
                var redirectUri = context.Properties?.RedirectUri ?? string.Empty;
                context.Response.Redirect(redirectUri);
                context.HandleResponse();
                return Task.CompletedTask;
            },
        };
    })
    .AddFacebook(facebookOptions =>
    {
        facebookOptions.UsePkce = true;
        facebookOptions.AppId = appSetting?.Authentication?.Facebook?.ClientId ?? string.Empty;
        facebookOptions.AppSecret = appSetting?.Authentication?.Facebook?.ClientSecret ?? string.Empty;
        facebookOptions.CallbackPath = appSetting?.Authentication?.Facebook?.Callback ?? string.Empty;
        facebookOptions.SendAppSecretProof = true;
        facebookOptions.Fields.Add("id");
        facebookOptions.Fields.Add("email");
        facebookOptions.Fields.Add("name");
        facebookOptions.Fields.Add("birthday");
        facebookOptions.Fields.Add("gender");
        facebookOptions.Events = new OAuthEvents
        {
            OnRedirectToAuthorizationEndpoint = context =>
            {
                if (appSetting?.Authentication?.Facebook?.RedirectUriParams != null)
                {
                    appSetting?.Authentication?.Facebook?.RedirectUriParams.ForEach(param =>
                    {
                        context.Response.Redirect(context.RedirectUri + param);
                    });
                }

                return Task.CompletedTask;
            },
            OnRemoteFailure = context =>
            {
                var redirectUri = context.Properties?.RedirectUri ?? string.Empty;
                context.Response.Redirect(redirectUri);
                context.HandleResponse();
                return Task.CompletedTask;
            },
        };
    })
    .AddOAuth<OAuthOptions, ZaloOAuthHandler>(LoginProvider.Zalo, options =>
    {
        options.UsePkce = true;
        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.ClientId = appSetting?.Authentication?.Zalo?.ClientId ?? string.Empty;
        options.ClientSecret = appSetting?.Authentication?.Zalo?.ClientSecret ?? string.Empty;
        options.CallbackPath = appSetting?.Authentication?.Zalo?.Callback ?? string.Empty;
        options.AuthorizationEndpoint = appSetting?.Authentication?.Zalo?.AuthorizationEndpoint ?? string.Empty;
        options.TokenEndpoint = appSetting?.Authentication?.Zalo?.TokenEndpoint ?? string.Empty;
        options.UserInformationEndpoint = appSetting?.Authentication?.Zalo?.UserInformationEndpoint ?? string.Empty;
        options.SaveTokens = true;

        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, UserInfoFields.Id);
        options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, UserInfoFields.Name);
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, UserInfoFields.Email);
        options.ClaimActions.MapJsonKey(ClaimTypes.MobilePhone, UserInfoFields.Phone);
        options.ClaimActions.MapJsonKey(ClaimTypes.Gender, UserInfoFields.Gender);
        options.ClaimActions.MapJsonKey(ClaimTypes.DateOfBirth, UserInfoFields.Birthday);

        options.Scope.Add("scope.userInfo");
        options.Scope.Add("scope.userLocation");
        options.Scope.Add("scope.userPhonenumber");

        options.Events = new OAuthEvents
        {
            OnAccessDenied = context =>
            {
                return Task.CompletedTask;
            },
            OnTicketReceived = context =>
            {
                return Task.CompletedTask;
            },
            OnRemoteFailure = context =>
            {
                var redirectUri = context.Properties?.RedirectUri ?? string.Empty;
                context.Response.Redirect(redirectUri);
                context.HandleResponse();
                return Task.CompletedTask;
            },
            OnCreatingTicket = async context =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                request.Headers.Add(TokenTypes.AccessToken, context.AccessToken);

                var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                response.EnsureSuccessStatusCode();

                using var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                context.RunClaimActions(user.RootElement);
                context.Properties.Items[LoginProvider.Name] = context.Scheme.ToString();
            },
            OnRedirectToAuthorizationEndpoint = context =>
            {
                var uri = new UriBuilder(context.RedirectUri);
                var query = QueryHelpers.ParseQuery(uri.Query);

                if (query.ContainsKey(OAuthFields.ClientId))
                {
                    var appId = query[OAuthFields.ClientId];
                    query.Remove(OAuthFields.ClientId);
                    query[OAuthFields.AppId] = appId;
                }

                uri.Query = new QueryBuilder(query.SelectMany(kvp => kvp.Value, (kvp, v) => new KeyValuePair<string, string>(kvp.Key, v))).ToQueryString().ToString();

                context.Response.Redirect(uri.ToString());
                return Task.CompletedTask;
            }
        };
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

builder.Services.AddSingleton<ICorsPolicyService>((container) =>
{
    var logger = container.GetRequiredService<ILogger<DefaultCorsPolicyService>>();
    return new DefaultCorsPolicyService(logger)
    {
        AllowAll = true
    };
});

builder.WebHost.UseKestrel();

var fordwardedHeaderOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    RequireHeaderSymmetry = false
};
fordwardedHeaderOptions.KnownNetworks.Clear();
fordwardedHeaderOptions.KnownProxies.Clear();
builder.Services.Configure<ForwardedHeadersOptions>(x => x = fordwardedHeaderOptions);


builder.Services.AddMvc();
builder.Services.AddMvcCore();
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddLocalApiAuthentication();
builder.Services.AddHttpsRedirection(opt => opt.HttpsPort = 443);

#endregion AddOpenIdServices

//Repository
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserTokenRepository, UserTokenRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<IParentRepository, ParentRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IUserOtpCodeRepository, UserOtpCodeRepository>();
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
builder.Services.AddScoped<IUserSchoolRepository, UserSchoolRepository>();
builder.Services.AddScoped<ISchoolImportHistoryRepository, SchoolImportHistoryRepository>();
builder.Services.AddScoped<IPermissionGroupRepository, PermissionGroupRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IRoleClaimRepository, RoleClaimRepository>();
builder.Services.AddScoped<IUserGroupRepository, UserGroupRepository>();
builder.Services.AddScoped<IUserGroupMemberShipRepository, UserGroupMemberShipRepository>();
builder.Services.AddScoped<IEventManagerRepository, EventManagerRepository>();
builder.Services.AddScoped<IStudentEventLearningRecordRepository, StudentEventLearningRecordRepository>();
builder.Services.AddScoped<IStudentEditHistoryRepository, StudentEditHistoryRepository>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();

// Event
builder.Services.AddTransient<IEventSink, TokenIssuedEventHandler>();

// Queue
builder.Services.AddScoped<LeaderBoardPublisher>();
builder.Services.AddScoped<QuestBoardPublisher>();
builder.Services.AddScoped<NotificationMessagePublisher>();
builder.Services.AddScoped<CreateTokenHistoryPublisher>();
builder.Services.AddScoped<CreateStudentsFromFilePublisher>();
builder.Services.AddScoped<SendStudentsFromFilePublisher>();

//Common
builder.Services.AddScoped<SaveOtpCodeConverter>();

//Refit
builder.AddRefitClients(typeof(ISenderService), appSetting?.Services?.SenderApiUrl);
builder.AddRefitClients(typeof(IOrderService), appSetting?.Services?.OrderApiUrl);
builder.AddRefitClients(typeof(IInteractionService), appSetting?.Services?.InteractionApiUrl);
builder.AddRefitClients(typeof(ITrainingService), appSetting?.Services?.ClassApiUrl);
builder.AddRefitClients(typeof(ILmsCourseService), appSetting?.Services?.LmsCourseApiUrl);
builder.AddRefitClients(typeof(ISystemService), appSetting?.Services?.SystemApiUrl);

builder.AddMassTransit(appSetting);

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
    MinimumSameSitePolicy = SameSiteMode.None,
    Secure = CookieSecurePolicy.Always
});

app.UseCors();
app.UseCors(Settings.CorsPolicy);

app.UseForwardedHeaders(fordwardedHeaderOptions);
app.UseDefaultServices();

#region Initialized Database

using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()!.CreateScope())
{
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

// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Authentication.Infrastructure.Configs;
using Fsel.Common.Constants;
using Fsel.Core.Extensions;
using Fsel.Identity.Application.Events;
using Fsel.Identity.Authentication.Extensions;
using Fsel.Identity.Infrastructure;
using Fsel.Identity.Infrastructure.Common;
using Fsel.Identity.Infrastructure.ValueSettings;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var appSetting = builder.AddAppSettings<AppSetting>();

builder.AddServices(appSetting);
builder.AddDbContexts<UserDbContext>();

builder.AddOIDC(appSetting);

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

#if DEBUG
var env = builder.Environment;
if (env.IsDevelopment() &&
    builder.Configuration["Urls"]?.Contains("https://fsel-auth-dev.fsel.edu.vn:443") == true)
{
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.ListenAnyIP(443, listenOptions =>
        {
            listenOptions.UseHttps("./Resources/CertificateSSL/fsel-auth-dev.fsel.edu.vn.pfx", "");
        });
    });
}
#endif

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

builder.AddRepositories();
builder.AddHandlers();

// Event
builder.Services.AddTransient<IEventSink, TokenIssuedEventHandler>();

builder.AddQueue();

builder.Services.AddScoped<SaveOtpCodeConverter>();

builder.AddExternalServices(appSetting);

builder.AddMassTransit(appSetting);

//App config
var app = builder.Build();
app.UseLanguages();
app.UseStaticFiles();
app.UseCertificateForwarding();
app.UseAuthentication();
app.UseAuthorization();
app.UseIdentityServer();
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

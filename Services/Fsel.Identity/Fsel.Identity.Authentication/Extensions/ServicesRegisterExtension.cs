// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Authentication.Extensions
{
    using System.Security.Claims;
    using System.Text.Json;
    using Fsel.Authentication.Infrastructure.Configs;
    using Fsel.Common.Constants;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Application.Events;
    using Fsel.Identity.Application.Handlers.Implementations;
    using Fsel.Identity.Application.Handlers.Interfaces;
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
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OAuth;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.EntityFrameworkCore;
    using static IdentityServer4.IdentityServerConstants;

    public static class ServicesRegisterExtension
    {
        public static WebApplicationBuilder AddHandlers(this WebApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Services.AddScoped<IUserRegisterHandler, UserRegisterHandler>();
            builder.Services.AddScoped<IForgotPasswordHandler, ForgotPasswordHandler>();
            builder.Services.AddScoped<IOtpHandlerPipeline<CheckBlockOtpHandler>, CheckBlockOtpHandler>();
            builder.Services.AddScoped<IOtpHandlerPipeline<CheckBlockSendOtpHandler>, CheckBlockSendOtpHandler>();
            builder.Services.AddScoped<IOtpHandlerPipeline<SendOtpHandler>, SendOtpHandler>();
            builder.Services.AddScoped<IOtpHandlerPipeline<SendOtpResultHandler>, SendOtpResultHandler>();
            builder.Services.AddScoped<IOtpHandlerPipeline<VerifyOtpHandler>, VerifyOtpHandler>();
            builder.Services.AddScoped<IOtpHandlerPipeline<VerifyOtpResultHandler>, VerifyOtpResultHandler>();
            builder.Services.AddScoped<IOtpPipelineFactory, OtpPipelineFactory>();
            return builder;
        }

        public static WebApplicationBuilder AddRepositories(this WebApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
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
            builder.Services.AddScoped<IEventManagerRepository, EventManagerRepository>();
            builder.Services.AddScoped<IStudentEventLearningRecordRepository, StudentEventLearningRecordRepository>();
            return builder;
        }

        public static WebApplicationBuilder AddQueue(this WebApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Services.AddScoped<LeaderBoardPublisher>();
            builder.Services.AddScoped<QuestBoardPublisher>();
            builder.Services.AddScoped<NotificationMessagePublisher>();
            builder.Services.AddScoped<CreateTokenHistoryPublisher>();
            builder.Services.AddScoped<CreateStudentsFromFilePublisher>();
            builder.Services.AddScoped<SendStudentsFromFilePublisher>();

            return builder;
        }

        public static WebApplicationBuilder AddExternalServices(this WebApplicationBuilder builder, AppSetting appSetting)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.AddRefitClients(typeof(ISenderService), appSetting?.Services?.SenderApiUrl);
            builder.AddRefitClients(typeof(IOrderService), appSetting?.Services?.OrderApiUrl);
            builder.AddRefitClients(typeof(IInteractionService), appSetting?.Services?.InteractionApiUrl);
            builder.AddRefitClients(typeof(ITrainingService), appSetting?.Services?.ClassApiUrl);
            builder.AddRefitClients(typeof(ILmsCourseService), appSetting?.Services?.LmsCourseApiUrl);
            builder.AddRefitClients(typeof(ISystemService), appSetting?.Services?.SystemApiUrl);

            return builder;
        }

        public static WebApplicationBuilder AddOIDC(this WebApplicationBuilder builder, AppSetting appSetting)
        {
            ArgumentNullException.ThrowIfNull(builder);
            var defaultConnString = builder.Configuration.GetConnectionString(Settings.DefaultConnection);
            var assembly = typeof(UserDbContext).Assembly.GetName().Name;
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

            return builder;
        }
    }
}

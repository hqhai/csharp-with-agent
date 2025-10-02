// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Authentication.Infrastructure.Configs
{
    using System.Collections.Generic;
    using Fsel.Identity.Domain.Constants;
    using IdentityModel;
    using IdentityServer4;
    using IdentityServer4.Models;

    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
           new IdentityResource[]
           {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResources.Phone(),
                new IdentityResources.Address(),
                new IdentityResource("roles", "Roles", new[]{ JwtClaimTypes.Role })
           };

        public static IEnumerable<ApiScope> ApiScopes =>
           new[] { new ApiScope("api"), };


        public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
            {
                new ApiResource("api", "My API", new[]{ JwtClaimTypes.Role })
            };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                new Client
                {
                    ClientId = "app.fsel.angular",

                    AllowedGrantTypes = GrantTypes.CodeAndImpersonation,

                    ClientSecrets =
                    {
                        new Secret("app.fsel.angular_secret".Sha256())
                    },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Phone,
                        IdentityServerConstants.StandardScopes.Address,
                        IdentityServerSettings.AllowedScopes.Roles,
                        IdentityServerSettings.AllowedScopes.Api
                    },

                    RequireClientSecret = false,
                    RequirePkce = true,
                    RequireConsent = false,
                    RedirectUris =
                    {
                        "https://localhost:4400",
                        "http://localhost:4400",
                        "https://lms-dev.fsel.edu.vn",
                        "http://lms-dev.fsel.edu.vn",
                        "https://lms-dev-tenant-1.fsel.edu.vn",
                        "http://lms-dev-tenant-1.fsel.edu.vn"
                        "https://lms-dev-ufm.fsel.edu.vn",
                        "http://lms-dev-ufm.fsel.edu.vn"
                    },
                    PostLogoutRedirectUris =
                    {
                        "https://localhost:4400",
                        "http://localhost:4400",
                        "https://lms-dev.fsel.edu.vn",
                        "http://lms-dev.fsel.edu.vn",
                        "https://lms-dev-ufm.fsel.edu.vn",
                        "http://lms-dev-ufm.fsel.edu.vn",
                        "https://localhost:4400/auth/login",
                        "http://localhost:4400/auth/login",
                        "https://lms-dev.fsel.edu.vn/auth/login",
                        "http://lms-dev.fsel.edu.vn/auth/login",
                        "https://lms-dev-tenant-1.fsel.edu.vn/auth/login",
                        "http://lms-dev-tenant-1.fsel.edu.vn/auth/login"
                        "https://lms-dev-ufm.fsel.edu.vn/auth/login",
                        "http://lms-dev-ufm.fsel.edu.vn/auth/login"
                    },

                    AllowOfflineAccess = true,
                    AccessTokenLifetime = 3600,
                    AllowAccessTokensViaBrowser = true,
                    UpdateAccessTokenClaimsOnRefresh = true,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly, // Cấu hình việc sử dụng lại refresh token
                    RefreshTokenExpiration = TokenExpiration.Sliding, // Cấu hình thời gian sống của refresh token
                    SlidingRefreshTokenLifetime = 1209600, // Cấu hình thời gian sống cho refresh token
                    AlwaysIncludeUserClaimsInIdToken = true, //hiển thị claims trong token
                },
                new Client
                {
                    ClientId = "app.fsel.angular.lcms",

                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        new Secret("app.fsel.angular.lcms_secret".Sha256())
                    },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Phone,
                        IdentityServerConstants.StandardScopes.Address,
                        IdentityServerSettings.AllowedScopes.Roles,
                        IdentityServerSettings.AllowedScopes.Api
                    },

                    RequireClientSecret = false,
                    RequirePkce = true,
                    RequireConsent = false,
                    RedirectUris =
                    {
                        "https://localhost:4200",
                        "http://localhost:4200",
                        "http://lcms-web-dev.fsel.edu.vn",
                        "https://lcms-web-dev.fsel.edu.vn",
                        "http://lcms-web-dev-tenant-1.fsel.edu.vn",
                        "https://lcms-web-dev-tenant-1.fsel.edu.vn"
                        "http://lcms-web-dev-ufm.fsel.edu.vn",
                        "https://lcms-web-dev-ufm.fsel.edu.vn"
                    },
                    PostLogoutRedirectUris =
                    {
                        "https://localhost:4200",
                        "http://localhost:4200",
                        "http://lcms-web-dev.fsel.edu.vn",
                        "https://lcms-web-dev.fsel.edu.vn",
                        "http://lcms-web-dev-tenant-1.fsel.edu.vn",
                        "https://lcms-web-dev-tenant-1.fsel.edu.vn"
                        "http://lcms-web-dev-ufm.fsel.edu.vn",
                        "https://lcms-web-dev-ufm.fsel.edu.vn"
                    },

                    AllowOfflineAccess = true,
                    AllowAccessTokensViaBrowser = true,
                    UpdateAccessTokenClaimsOnRefresh = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                },
                new Client
                {
                    ClientId = "app.fsel.angular.lmsadmin",

                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndImpersonation,

                    ClientSecrets =
                    {
                        new Secret("app.fsel.angular.lmsadmin_secret".Sha256())
                    },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Phone,
                        IdentityServerConstants.StandardScopes.Address,
                        IdentityServerSettings.AllowedScopes.Roles,
                        IdentityServerSettings.AllowedScopes.Api
                    },

                    RequireClientSecret = false,
                    RequirePkce = true,
                    RequireConsent = false,
                    RedirectUris =
                    {
                        "https://localhost:4300",
                        "http://localhost:4300",
                        "http://lms-admin-dev.fsel.edu.vn",
                        "https://lms-admin-dev.fsel.edu.vn",
                        "http://lms-admin-dev-tenant-1.fsel.edu.vn",
                        "https://lms-admin-dev-tenant-1.fsel.edu.vn"
                        "http://lms-admin-dev-ufm.fsel.edu.vn",
                        "https://lms-admin-dev-ufm.fsel.edu.vn"
                    },
                    PostLogoutRedirectUris =
                    {
                        "https://localhost:4300",
                        "http://localhost:4300",
                        "http://lms-admin-dev.fsel.edu.vn",
                        "https://lms-admin-dev.fsel.edu.vn",
                        "http://lms-admin-dev-tenant-1.fsel.edu.vn",
                        "https://lms-admin-dev-tenant-1.fsel.edu.vn"
                        "http://lms-admin-dev-ufm.fsel.edu.vn",
                        "https://lms-admin-dev-ufm.fsel.edu.vn"
                    },

                    AllowOfflineAccess = true,
                    AllowAccessTokensViaBrowser = true,
                    UpdateAccessTokenClaimsOnRefresh = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                },
                new Client
                {
                    ClientId = "app.fsel.flutter",
                    AllowedGrantTypes = GrantTypes.Code,

                    ClientSecrets =
                    {
                        new Secret("app.fsel.flutter_secret".Sha256())
                    },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Phone,
                        IdentityServerConstants.StandardScopes.Address,
                        IdentityServerSettings.AllowedScopes.Roles,
                        IdentityServerSettings.AllowedScopes.Api
                    },

                    RequireClientSecret = false,
                    RequirePkce = true,
                    RedirectUris = { "https://lms.fsel.edu.vn/login", "http://lms.fsel.edu.vn/login" },
                    PostLogoutRedirectUris = { "https://lms.fsel.edu.vn/login", "http://lms.fsel.edu.vn/login" },

                    AllowOfflineAccess = true,
                    RequireConsent = false,
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true, //hiển thị claims trong token
                },
                new Client
                {
                    ClientId = "com.fsel.lmsapp.uat",
                    AllowedGrantTypes = GrantTypes.Code,

                    ClientSecrets =
                    {
                        new Secret("com.fsel.lmsapp.uat_secret".Sha256())
                    },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Phone,
                        IdentityServerConstants.StandardScopes.Address,
                        IdentityServerSettings.AllowedScopes.Roles,
                        IdentityServerSettings.AllowedScopes.Api
                    },

                    RequireClientSecret = false,
                    RequirePkce = false,
                    RedirectUris = { "https://lms-testing.fsel.edu.vn/auth/login", "http://lms-testing.fsel.edu.vn/auth/login", "fsel-uat://lms-testing.fsel.edu.vn" },
                    PostLogoutRedirectUris = { "https://lms-testing.fsel.edu.vn/auth/login", "http://lms-testing.fsel.edu.vn/auth/login", "fsel-uat://lms-testing.fsel.edu.vn" },

                    AllowOfflineAccess = true,
                    AccessTokenLifetime = 3600,
                    AllowAccessTokensViaBrowser = true,
                    UpdateAccessTokenClaimsOnRefresh = true,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly, // Cấu hình việc sử dụng lại refresh token
                    RefreshTokenExpiration = TokenExpiration.Sliding, // Cấu hình thời gian sống của refresh token
                    SlidingRefreshTokenLifetime = 1209600, // Cấu hình thời gian sống cho refresh token
                    AlwaysIncludeUserClaimsInIdToken = true, //hiển thị claims trong token
                },
                new Client
                {
                    ClientId = "test.fsel.password",

                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        new Secret("test.fsel.password_secret".Sha256())
                    },

                    PostLogoutRedirectUris = { "https://localhost:4400", "https://identityserver4.readthedocs.io/en/latest/endpoints/endsession.html" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Phone,
                        IdentityServerConstants.StandardScopes.Address,
                        IdentityServerSettings.AllowedScopes.Roles,
                        IdentityServerSettings.AllowedScopes.Api
                    },

                    AllowOfflineAccess = true, // Cho phép sử dụng refresh token
                    AccessTokenLifetime = 3600,
                    UpdateAccessTokenClaimsOnRefresh = true,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly, // Cấu hình việc sử dụng lại refresh token
                    RefreshTokenExpiration = TokenExpiration.Sliding, // Cấu hình thời gian sống của refresh token
                    SlidingRefreshTokenLifetime = 1209600, // Cấu hình thời gian sống cho refresh token
                    AlwaysIncludeUserClaimsInIdToken = true, //hiển thị claims trong token
                },
                new Client
                {
                    ClientId = "test.fsel.impersonation",

                    AllowedGrantTypes = GrantTypes.Impersonation,

                    ClientSecrets =
                    {
                        new Secret("test.fsel.impersonation_secret".Sha256())
                    },

                    PostLogoutRedirectUris = { "https://localhost:4400", "https://identityserver4.readthedocs.io/en/latest/endpoints/endsession.html" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Phone,
                        IdentityServerConstants.StandardScopes.Address,
                        IdentityServerSettings.AllowedScopes.Roles,
                        IdentityServerSettings.AllowedScopes.Api
                    },

                    AllowOfflineAccess = true, // Cho phép sử dụng refresh token
                    AccessTokenLifetime = 3600,
                    UpdateAccessTokenClaimsOnRefresh = true,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly, // Cấu hình việc sử dụng lại refresh token
                    RefreshTokenExpiration = TokenExpiration.Sliding, // Cấu hình thời gian sống của refresh token
                    SlidingRefreshTokenLifetime = 1209600, // Cấu hình thời gian sống cho refresh token
                    AlwaysIncludeUserClaimsInIdToken = true, //hiển thị claims trong token
                },
                new Client
                {
                    ClientId = "test.fsel.mvc",
                    ClientName = "MVC Client",
                    AllowedGrantTypes = GrantTypes.Code,

                    ClientSecrets =
                    {
                        new Secret("test.fsel.mvc_secret".Sha256())
                    },

                    RedirectUris = { "https://localhost:7088/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:7088/signout-callback-oidc" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Phone,
                        IdentityServerConstants.StandardScopes.Address,
                        IdentityServerSettings.AllowedScopes.Roles,
                        IdentityServerSettings.AllowedScopes.Api
                    },

                    AllowOfflineAccess = true,
                    RequirePkce = true,
                    RequireConsent = false,
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true, //hiển thị claims trong token

                    //Claims = new ClientClaim[]
                    //{
                    //    new ClientClaim(JwtClaimTypes.Role, "Admin")
                    //},
                },
                new Client
                {
                    ClientId = "test.fsel.swagger",
                    ClientName = "Swagger UI",
                    AllowedGrantTypes = GrantTypes.Code,

                    ClientSecrets =
                    {
                        new Secret("test.fsel.swagger_secret".Sha256())
                    },

                    RedirectUris =
                    {
                        "https://localhost:7203/swagger/oauth2-redirect.html",
                        "https://localhost:7202/swagger/oauth2-redirect.html",
                        "https://localhost:7204/swagger/oauth2-redirect.html",
                        "https://fsel-gateway-dev.fsel.edu.vn/swagger/oauth2-redirect.html",
                        "https://fsel-lcms-dev.fsel.edu.vn/swagger/oauth2-redirect.html"
                    },
                    PostLogoutRedirectUris =
                    {
                        "https://localhost:7203/swagger/oauth2-redirect.html",
                        "https://localhost:7202/swagger/oauth2-redirect.html",
                        "https://localhost:7204/swagger/oauth2-redirect.html",
                        "https://fsel-gateway-dev.fsel.edu.vn/swagger/oauth2-redirect.html",
                        "https://fsel-lcms-dev.fsel.edu.vn/swagger/oauth2-redirect.html"
                    },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Phone,
                        IdentityServerConstants.StandardScopes.Address,
                        IdentityServerSettings.AllowedScopes.Roles,
                        IdentityServerSettings.AllowedScopes.Api
                    },

                    AllowOfflineAccess = true,
                    RequirePkce = false,
                    RequireConsent = false,
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true, //hiển thị claims trong token

                    //Claims = new ClientClaim[]
                    //{
                    //    new ClientClaim(JwtClaimTypes.Role, "Admin")
                    //},
                }
            };
    }
}

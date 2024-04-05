// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Authentication.Infrastructure.Configs
{
    using System.Collections.Generic;
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
                new IdentityResource("roles", "Roles", new[]{ JwtClaimTypes.Role, JwtClaimTypes.NickName })
           };

        public static IEnumerable<ApiScope> ApiScopes =>
           new[] { new ApiScope("api"), };


        public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
            {
                new ApiResource("api", "My API", new[]{ JwtClaimTypes.Role, JwtClaimTypes.NickName })
            };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                new Client
                {
                    ClientId = "app.fsel.angular",

                    AllowedGrantTypes = GrantTypes.Code,

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
                        "roles",
                        "api"
                    },

                    RequireClientSecret = false,
                    RequirePkce = true,
                    RedirectUris = { "https://localhost:4400", "http://localhost:4400" },
                    PostLogoutRedirectUris = { "https://localhost:4400", "http://localhost:4400" },

                    AllowOfflineAccess = true,
                    RequireConsent = false,
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true, //hiển thị claims trong token
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
                        "roles",
                        "api"
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
                        "roles",
                        "api"
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
                    ClientId = "test.fsel.password",

                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        new Secret("test.fsel.password_secret".Sha256())
                    },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Phone,
                        IdentityServerConstants.StandardScopes.Address,
                        "roles",
                        "api"
                    },

                    AllowOfflineAccess = true, // Cho phép sử dụng refresh token
                    RefreshTokenUsage = TokenUsage.OneTimeOnly, // Cấu hình việc sử dụng lại refresh token
                    RefreshTokenExpiration = TokenExpiration.Sliding, // Cấu hình thời gian sống của refresh token
                    SlidingRefreshTokenLifetime = 1209600, // Cấu hình thời gian sống cho refresh token
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

                    //RedirectUris = { "https://localhost:7088/Home/GetCode" },
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
                        "roles",
                        "api"
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
                }
            };
    }
}

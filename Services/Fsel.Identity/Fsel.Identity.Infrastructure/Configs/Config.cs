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
                    ClientId = "app.angular",

                    AllowedGrantTypes = GrantTypes.Code,

                    ClientSecrets =
                    {
                        new Secret("app.fsel.angular_secret".Sha256())
                    },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Phone,
                        IdentityServerConstants.StandardScopes.Address,
                        "roles",
                        "api"
                    },

                    AllowOfflineAccess = true,
                },
                new Client
                {
                    ClientId = "app.flutter",
                    AllowedGrantTypes = GrantTypes.Code,

                    ClientSecrets =
                    {
                        new Secret("app.fsel.flutter_secret".Sha256())
                    },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Phone,
                        IdentityServerConstants.StandardScopes.Address,
                        "roles",
                        "api"
                    },

                    AllowOfflineAccess = true,
                },
                new Client
                {
                    ClientId = "test.password",

                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        new Secret("test.fsel.password_secret".Sha256())
                    },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Phone,
                        IdentityServerConstants.StandardScopes.Address,
                        "roles",
                        "api"
                    },

                    AllowOfflineAccess = true, // Cho phép sử dụng refresh token
                    //RefreshTokenUsage = TokenUsage.OneTimeOnly, // Cấu hình việc sử dụng lại refresh token
                    //RefreshTokenExpiration = TokenExpiration.Sliding, // Cấu hình thời gian sống của refresh token
                    //SlidingRefreshTokenLifetime = 1209600, // Cấu hình thời gian sống cho refresh token
                },
                new Client
                {
                    ClientId = "test.mvc",
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
                    AllowAccessTokensViaBrowser = false,
                    AlwaysIncludeUserClaimsInIdToken = true, //hiển thị claims trong token

                    //Claims = new ClientClaim[]
                    //{
                    //    new ClientClaim(JwtClaimTypes.Role, "Admin")
                    //},
                }
            };
    }
}

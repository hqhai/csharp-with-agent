// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using IdentityModel;
using Fsel.Core.Base.Interfaces;
using Fsel.Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.AspNetIdentity
{
    internal class UserClaimsFactory<TUser> : IUserClaimsPrincipalFactory<TUser>
        where TUser : UserEntity
    {
        private readonly Decorator<IUserClaimsPrincipalFactory<TUser>> _inner;
        private UserManager<TUser> _userManager;
        private readonly IServiceProvider _serviceProvider;

        public UserClaimsFactory(Decorator<IUserClaimsPrincipalFactory<TUser>> inner, UserManager<TUser> userManager, IServiceProvider serviceProvider)
        {
            _inner = inner;
            _userManager = userManager;
            _serviceProvider = serviceProvider;
        }

        public async Task<ClaimsPrincipal> CreateAsync(TUser user)
        {
            var principal = await _inner.Instance.CreateAsync(user);
            var identity = principal.Identities.First();

            var tenantProvider = _serviceProvider.GetService<ITenantProvider>();
            _userManager = tenantProvider != null ? await tenantProvider.CreateUserManagerAsync<TUser>(user.UserName) ?? _userManager : _userManager;
            if (!identity.HasClaim(x => x.Type == JwtClaimTypes.Subject))
            {
                var sub = await _userManager.GetUserIdAsync(user);
                identity.AddClaim(new Claim(JwtClaimTypes.Subject, sub));
            }

            var username = await _userManager.GetUserNameAsync(user);
            var usernameClaim = identity.FindFirst(claim => claim.Type == _userManager.Options.ClaimsIdentity.UserNameClaimType && claim.Value == username);
            if (usernameClaim != null)
            {
                identity.RemoveClaim(usernameClaim);
                identity.AddClaim(new Claim(JwtClaimTypes.PreferredUserName, username));
            }

            if (!identity.HasClaim(x=>x.Type == JwtClaimTypes.Name))
            {
                identity.AddClaim(new Claim(JwtClaimTypes.Name, username));
            }

            if (_userManager.SupportsUserEmail)
            {
                var email = await _userManager.GetEmailAsync(user);
                if (!String.IsNullOrWhiteSpace(email))
                {
                    identity.AddClaims(new[]
                    {
                        new Claim(JwtClaimTypes.Email, email),
                        new Claim(JwtClaimTypes.EmailVerified,
                            await _userManager.IsEmailConfirmedAsync(user) ? "true" : "false", ClaimValueTypes.Boolean)
                    });
                }
            }

            if (_userManager.SupportsUserPhoneNumber)
            {
                var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
                if (!String.IsNullOrWhiteSpace(phoneNumber))
                {
                    identity.AddClaims(new[]
                    {
                        new Claim(JwtClaimTypes.PhoneNumber, phoneNumber),
                        new Claim(JwtClaimTypes.PhoneNumberVerified,
                            await _userManager.IsPhoneNumberConfirmedAsync(user) ? "true" : "false", ClaimValueTypes.Boolean)
                    });
                }
            }

            return principal;
        }
    }
}

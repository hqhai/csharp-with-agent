// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.UserProfileService
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using IdentityModel;
    using IdentityServer4.AspNetIdentity;
    using IdentityServer4.Extensions;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Identity;

    public class UserProfileService : ProfileService<User>, IProfileService
    {
        private readonly Core.Base.Managers.UserManager<User> _userManager;
        private readonly Core.Base.Managers.RoleManager<Role> _roleManager;

        public UserProfileService(Core.Base.Managers.UserManager<User> usermanager, Core.Base.Managers.RoleManager<Role> roleManager, IUserClaimsPrincipalFactory<User> userClaimsPrincipalFactory)
            : base(usermanager, userClaimsPrincipalFactory)
        {
            _userManager = usermanager;
            _roleManager = roleManager;
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var user = await _userManager.GetUserAsync(context.Subject);

            if(user != null)
            {
                var claims = (await _userManager.GetClaimsAsync(user)).ToList();
                var roles = await _userManager.GetRolesAsync(user);

                foreach (var role in roles)
                {
                    claims.Add(new System.Security.Claims.Claim(JwtClaimTypes.Role, role));

                    var roleEntity = await _roleManager.FindByNameAsync(role);
                    if (roleEntity != null)
                    {
                        var roleClaims = await _roleManager.GetClaimsAsync(roleEntity);
                        claims.AddRange(roleClaims.Where(m => context.RequestedClaimTypes.Any(x => x.Equals(m.Type, StringComparison.Ordinal))));
                    }
                }

                context.IssuedClaims.AddRange(claims);
            }

            await base.GetProfileDataAsync(context);
        }

        public override async Task IsActiveAsync(IsActiveContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            var active = (user != null && (!user.LockoutEnabled || user.LockoutEnd == null)) ||
                         (user != null && user.LockoutEnabled && user.LockoutEnd != null &&
                          DateTime.UtcNow > user.LockoutEnd);

            context.IsActive = active;
        }
    }
}

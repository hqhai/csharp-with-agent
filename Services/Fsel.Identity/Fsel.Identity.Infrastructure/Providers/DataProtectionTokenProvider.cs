// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Providers
{
    using Fsel.Core.Entities;
    using Microsoft.AspNetCore.Identity;

    public static class DataProtectionTokenProvider
    {
        public const string TotpProviderName = "TotpProvider";

        public static IdentityBuilder AddTotpProvider(this IdentityBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            var userType = builder.UserType;
            var provider = typeof(TotpProvider<>).MakeGenericType(userType);
            return builder.AddTokenProvider(TotpProviderName, provider);
        }
    }

    public class TotpProviderOptions : DataProtectionTokenProviderOptions
    {
        public TotpProviderOptions()
        {
            // update the defaults
            Name = DataProtectionTokenProvider.TotpProviderName;
            TokenLifespan = TimeSpan.FromMinutes(1);
        }
    }

    public class TotpProvider<TUser> : TotpSecurityStampBasedTokenProvider<TUser>
    where TUser : UserEntity
    {
        public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
        {
            return Task.FromResult(false);
        }
    }
}

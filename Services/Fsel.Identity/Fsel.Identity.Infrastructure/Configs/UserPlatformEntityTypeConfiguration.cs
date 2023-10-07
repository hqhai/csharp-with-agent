// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    internal class UserPlatformEntityTypeConfiguration : IEntityTypeConfiguration<UserPlatform>
    {
        public void Configure(EntityTypeBuilder<UserPlatform> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.User)
                 .WithMany(b => b.UserPlatforms)
                 .HasForeignKey(p => p.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Platform)
                 .WithMany(b => b.UserPlatforms)
                 .HasForeignKey(p => p.PlatformId)
                 .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.Status)
                 .HasMaxLength(100)
                 .HasConversion(
                     v => v.ToString(),
                     v => v.EnumParse<EnumUserPlatformStatus>());
        }
    }
}

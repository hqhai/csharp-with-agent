// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;

    internal class PlatformEntityTypeConfiguration : IEntityTypeConfiguration<Platform>
    {
        public void Configure(EntityTypeBuilder<Platform> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.Type)
                 .HasMaxLength(100)
                 .HasConversion(
                     v => v.ToString(),
                     v => v.EnumParse<EnumPlatformType>());

            builder.Property(e => e.Code)
                 .HasMaxLength(100)
                 .HasConversion(
                     v => v.ToString(),
                     v => v.EnumParse<EnumPlatformCode>());
        }
    }
}

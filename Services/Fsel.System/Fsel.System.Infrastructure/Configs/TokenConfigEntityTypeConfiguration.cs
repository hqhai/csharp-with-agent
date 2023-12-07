// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using global::System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TokenConfigEntityTypeConfiguration : IEntityTypeConfiguration<TokenConfig>
    {
        public void Configure(EntityTypeBuilder<TokenConfig> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Feature)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumTokenFeature>());

            builder.Property(e => e.Mission)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumTokenMission>());
        }
    }
}

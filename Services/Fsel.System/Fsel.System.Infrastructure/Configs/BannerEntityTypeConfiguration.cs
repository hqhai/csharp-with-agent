// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BannerEntityTypeConfiguration : IEntityTypeConfiguration<Banner>
    {
        public void Configure(EntityTypeBuilder<Banner> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.Type)
                   .HasMaxLength(100)
                   .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumBannerType>());

            builder.Property(e => e.BannerFrequency)
                   .HasMaxLength(100)
                   .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumBannerFrequency>());
        }
    }
}

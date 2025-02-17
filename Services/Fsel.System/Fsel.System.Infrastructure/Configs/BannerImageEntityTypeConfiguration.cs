// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BannerImageEntityTypeConfiguration : IEntityTypeConfiguration<BannerImage>
    {
        public void Configure(EntityTypeBuilder<BannerImage> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.RouteScreen)
                   .HasMaxLength(100)
                   .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumBannerLink>());

            builder.HasOne(a => a.Banner)
                   .WithMany(b => b.BannerImages)
                   .HasForeignKey(b => b.BannerId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

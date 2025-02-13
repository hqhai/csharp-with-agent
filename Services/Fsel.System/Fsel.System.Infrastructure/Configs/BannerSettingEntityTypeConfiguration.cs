// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.System.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BannerSettingEntityTypeConfiguration : IEntityTypeConfiguration<BannerSetting>
    {
        public void Configure(EntityTypeBuilder<BannerSetting> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
        }
    }
}

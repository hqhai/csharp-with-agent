// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Configs
{
    using System;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    internal class AvatarImageEntityTypeConfiguration : IEntityTypeConfiguration<AvatarImage>
    {
        public void Configure(EntityTypeBuilder<AvatarImage> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(b => b.Level).HasDefaultValue(1);
        }
    }
}

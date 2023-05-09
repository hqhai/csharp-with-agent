// Copyright (c) Atlantic. All rights reserved.

using Fsel.Shared.Enums;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class PlacementTestEntityTypeConfiguration : IEntityTypeConfiguration<PlacementTest>
    {
        public void Configure(EntityTypeBuilder<PlacementTest> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.PlacementTestLevel)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumPlacementTestLevel>());
        }
    }
}

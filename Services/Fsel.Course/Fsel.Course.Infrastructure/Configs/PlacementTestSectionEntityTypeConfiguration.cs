// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class PlacementTestSectionEntityTypeConfiguration : IEntityTypeConfiguration<PlacementTestSection>
    {
        public void Configure(EntityTypeBuilder<PlacementTestSection> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.SectionGroup)
                .WithMany(b => b.PlacementTestSections)
                .HasForeignKey(b => b.SectionGroupId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(a => a.PlacementTest)
                .WithMany(b => b.PlacementTestSections)
                .HasForeignKey(b => b.PlacementTestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

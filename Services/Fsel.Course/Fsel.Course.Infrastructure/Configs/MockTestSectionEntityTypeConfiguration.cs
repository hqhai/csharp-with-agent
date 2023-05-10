// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class MockTestSectionEntityTypeConfiguration : IEntityTypeConfiguration<MockTestSection>
    {
        public void Configure(EntityTypeBuilder<MockTestSection> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.SectionGroup)
                .WithMany(b => b.MockTestSections)
                .HasForeignKey(b => b.SectionGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.MockTest)
                .WithMany(b => b.MockTestSections)
                .HasForeignKey(b => b.SectionGroupId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}

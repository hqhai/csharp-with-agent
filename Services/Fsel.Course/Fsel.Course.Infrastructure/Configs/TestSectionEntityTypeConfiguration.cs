// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TestSectionEntityTypeConfiguration : IEntityTypeConfiguration<TestSection>
    {
        public void Configure(EntityTypeBuilder<TestSection> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Test)
                   .WithMany(b => b.TestSections)
                   .HasForeignKey(b => b.TestId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Parent)
                   .WithMany(b => b.TestSections)
                   .HasForeignKey(p => p.ParentId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.Property(e => e.LayoutType)
                .HasMaxLength(30)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumTestLayoutType>());
        }
    }
}

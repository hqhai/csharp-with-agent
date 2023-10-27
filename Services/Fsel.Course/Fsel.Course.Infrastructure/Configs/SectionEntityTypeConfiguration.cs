// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class SectionEntityTypeConfiguration : IEntityTypeConfiguration<Section>
    {
        public void Configure(EntityTypeBuilder<Section> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.SectionGroup)
                .WithMany(b => b.Sections)
                .HasForeignKey(b => b.SectionGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

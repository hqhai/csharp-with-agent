// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class FinalTestSectionEntityTypeConfiguration : IEntityTypeConfiguration<FinalTestSection>
    {
        public void Configure(EntityTypeBuilder<FinalTestSection> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.FinalTest)
                .WithMany(b => b.FinalTestSections)
                .HasForeignKey(b => b.FinalTestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.SectionGroup)
                .WithMany(b => b.FinalTestSections)
                .HasForeignKey(b => b.SectionGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

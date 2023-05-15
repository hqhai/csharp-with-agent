// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class PlacementTestSectionResultEntityTypeConfiguration : IEntityTypeConfiguration<PlacementTestSectionResult>
    {
        public void Configure(EntityTypeBuilder<PlacementTestSectionResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.PlacementTestSection)
                .WithMany(b => b.PlacementTestSectionResults)
                .HasForeignKey(b => b.PlacementTestSectionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}

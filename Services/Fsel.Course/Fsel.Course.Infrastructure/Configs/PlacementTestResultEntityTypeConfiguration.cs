// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class PlacementTestResultEntityTypeConfiguration : IEntityTypeConfiguration<PlacementTestResult>
    {
        public void Configure(EntityTypeBuilder<PlacementTestResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.PlacementTest)
                .WithMany(b => b.PlacementTestResults)
                .HasForeignKey(b => b.PlacementTestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

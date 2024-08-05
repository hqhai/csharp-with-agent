// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class PlacementTestResultEntityTypeConfiguration : IEntityTypeConfiguration<PlacementTestResult>
    {
        public void Configure(EntityTypeBuilder<PlacementTestResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Level)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumPlacementTestLevel>());

            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumResultStatus>());

            builder.HasOne(a => a.PlacementTest)
              .WithMany(b => b.PlacementTestResults)
              .HasForeignKey(b => b.PlacementTestId)
              .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => new { c.PlacementTestId, c.StudentId }).IsUnique();
        }
    }
}

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
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumPlacementTestLevel>());

            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumResultStatus>());

            builder.HasOne(a => a.PlacementTest)
              .WithMany(b => b.PlacementTestResults)
              .HasForeignKey(b => b.PlacementTestId)
              .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.PlacementTestGroupResult)
                  .WithMany(b => b.PlacementTestResults)
                  .HasForeignKey(b => b.PlacementTestGroupResultId)
                  .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.StepFlow)
                  .WithMany(b => b.PlacementTestResults)
                  .HasForeignKey(b => b.StepFlowId)
                  .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.ActionFlow)
                  .WithMany(b => b.PlacementTestResults)
                  .HasForeignKey(b => b.ActionFlowId)
                  .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(c => new { c.PlacementTestId, c.StudentId }).IsUnique().HasFilter("PlacementTestId IS NOT NULL AND [IsDeleted] = 0");
            builder.HasIndex(c => new { c.Status, c.StudentId });
            builder.HasIndex(c => new { c.Level, c.StudentId });
            builder.HasIndex(c => new { c.StudentId });
            builder.HasIndexIncludeAllProperties(c => new { c.CreatedUserId });
        }
    }
}

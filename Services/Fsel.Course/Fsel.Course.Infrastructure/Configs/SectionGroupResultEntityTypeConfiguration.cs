// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class SectionGroupResultEntityTypeConfiguration : IEntityTypeConfiguration<SectionGroupResult>
    {
        public void Configure(EntityTypeBuilder<SectionGroupResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                     .HasMaxLength(20)
                     .HasConversion(
                         v => v.ToString(),
                         v => v.EnumParse<EnumResultStatus>());

            builder.HasOne(a => a.SectionGroup)
                .WithMany(b => b.SectionGroupResults)
                .HasForeignKey(b => b.SectionGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.MockTestResult)
              .WithMany(b => b.SectionGroupResults)
              .HasForeignKey(b => b.MockTestResultId)
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.FinalTestResult)
             .WithMany(b => b.SectionGroupResults)
             .HasForeignKey(b => b.FinalTestResultId)
             .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.ExtraPracticeResult)
            .WithMany(b => b.SectionGroupResults)
            .HasForeignKey(b => b.ExtraPracticeResultId)
            .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.PlacementTestResult)
                  .WithMany(b => b.SectionGroupResults)
                  .HasForeignKey(b => b.PlacementTestResultId)
                  .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(c => new { c.SectionGroupId, c.MockTestResultId }).IsUnique().HasFilter("MockTestResultId IS NOT NULL AND [IsDeleted] = 0");
            builder.HasIndex(c => new { c.SectionGroupId, c.FinalTestResultId }).IsUnique().HasFilter("FinalTestResultId IS NOT NULL AND [IsDeleted] = 0");
            builder.HasIndex(c => new { c.SectionGroupId, c.PlacementTestResultId }).IsUnique().HasFilter("PlacementTestResultId IS NOT NULL AND [IsDeleted] = 0");
            builder.HasIndex(c => new { c.StudentId });
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class PlacementTestAnswerEntityTypeConfiguration : IEntityTypeConfiguration<PlacementTestAnswer>
    {
        public void Configure(EntityTypeBuilder<PlacementTestAnswer> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.PlacementTestResult)
                .WithMany(b => b.PlacementTestAnswers)
                .HasForeignKey(b => b.PlacementTestResultId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.SectionQuestion)
                .WithMany(b => b.PlacementTestAnswers)
                .HasForeignKey(b => b.SectionQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.SectionGroupResult)
                  .WithMany(b => b.PlacementTestAnswers)
                  .HasForeignKey(b => b.SectionGroupResultId)
                  .OnDelete(DeleteBehavior.NoAction);

            builder.Property(e => e.Status)
                   .HasMaxLength(20)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumAnswerStatus>());

            builder.HasIndex(c => new { c.PlacementTestResultId, c.SectionGroupResultId, c.SectionQuestionId }).IsUnique().HasFilter("SectionGroupResultId IS NOT NULL AND [IsDeleted] = 0");
            builder.HasIndex(c => new { c.PlacementTestResultId, c.SectionQuestionId });
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class HomeWorkExtraPracticeResultEntityTypeConfiguration : IEntityTypeConfiguration<HomeWorkExtraPracticeResult>
    {
        public void Configure(EntityTypeBuilder<HomeWorkExtraPracticeResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumResultStatus>());

            builder.Property(e => e.SubmissionCount)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumSubmissionCount>());

            builder.Property(e => e.WorkingStatus)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumWorkingStatus>());

            builder.HasOne(a => a.HomeWorkRetry)
                   .WithMany(b => b.HomeWorkExtraPracticeResults)
                   .HasForeignKey(b => b.HomeWorkRetryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.HomeWork)
                   .WithMany(b => b.HomeWorkExtraPracticeResults)
                   .HasForeignKey(b => b.HomeWorkId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => new { c.StudentId, c.HomeWorkId, c.HomeWorkRetryId, c.WorkingStatus, c.IsDeleted });

            builder.Property(x => x.Percent)
              .HasComputedColumnSql(@"CASE WHEN [CorrectTotal] > 0 THEN ROUND(([CorrectCount] * 100.0) / [CorrectTotal], 0) ELSE 0 END", stored: true)
              .ValueGeneratedOnAddOrUpdate()
              .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}

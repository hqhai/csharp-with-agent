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

    public class HomeWorkResultEntityTypeConfiguration : IEntityTypeConfiguration<HomeWorkResult>
    {
        public void Configure(EntityTypeBuilder<HomeWorkResult> builder)
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

            builder.HasOne(a => a.LessonResult)
              .WithMany(b => b.HomeWorkResults)
              .HasForeignKey(b => b.LessonResultId)
              .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.HomeWork)
                .WithMany(b => b.HomeWorkResults)
                .HasForeignKey(b => b.HomeWorkId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.LessonModule)
                .WithMany(b => b.HomeWorkResults)
                .HasForeignKey(p => p.LessonModuleId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(c => new { c.StudentId });
            builder.HasIndexIncludeAllProperties(c => new { c.StudentId });
            builder.HasIndex(c => new { c.LessonResultId, c.LessonModuleId, c.IsDeleted });

            builder.Property(x => x.Percent)
                   .HasComputedColumnSql(@"CASE WHEN [CorrectTotal] > 0 THEN ROUND(([CorrectCount] * 100.0) / [CorrectTotal], 0) ELSE 0 END", stored: true)
                   .ValueGeneratedOnAddOrUpdate()
                   .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}

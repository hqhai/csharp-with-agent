// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ExamPracticeSectionResultEntityTypeConfiguration : IEntityTypeConfiguration<ExamPracticeSectionResult>
    {
        public void Configure(EntityTypeBuilder<ExamPracticeSectionResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.Status)
                 .HasMaxLength(20)
                 .HasConversion(
                     v => v.ToString(),
                     v => v.EnumParse<EnumResultStatus>());

            builder.HasOne(a => a.ExamPracticeResult)
                .WithMany(b => b.ExamPracticeSectionResults)
                .HasForeignKey(b => b.ExamPracticeResultId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.ExamPracticeSection)
              .WithMany(b => b.ExamPracticeSectionResults)
              .HasForeignKey(b => b.ExamPracticeSectionId)
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.ParentExamPracticeSectionResult)
                .WithMany(b => b.ExamPracticeSectionResults)
                .HasForeignKey(b => b.ParentExamPracticeSectionResultId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

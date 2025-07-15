// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ExamPracticeAnswerEntityTypeConfiguration : IEntityTypeConfiguration<ExamPracticeAnswer>
    {
        public void Configure(EntityTypeBuilder<ExamPracticeAnswer> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.Status)
                 .HasMaxLength(20)
                 .HasConversion(
                     v => v.ToString(),
                     v => v.EnumParse<EnumAnswerStatus>());

            builder.HasOne(a => a.ExamPracticeResult)
                .WithMany(b => b.ExamPracticeAnswers)
                .HasForeignKey(b => b.ExamPracticeResultId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.ExamPracticeSectionResult)
               .WithMany(b => b.ExamPracticeAnswers)
               .HasForeignKey(b => b.ExamPracticeSectionResultId)
               .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.ExamPracticeSection)
               .WithMany(b => b.ExamPracticeAnswers)
               .HasForeignKey(b => b.ExamPracticeSectionId)
               .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Question)
              .WithMany(b => b.ExamPracticeAnswers)
              .HasForeignKey(b => b.QuestionId)
              .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

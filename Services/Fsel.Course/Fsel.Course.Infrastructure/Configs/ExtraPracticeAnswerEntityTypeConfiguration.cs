// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ExtraPracticeAnswerEntityTypeConfiguration : IEntityTypeConfiguration<ExtraPracticeAnswer>
    {
        public void Configure(EntityTypeBuilder<ExtraPracticeAnswer> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Question)
                .WithMany(b => b.ExtraPracticeAnswers)
                .HasForeignKey(b => b.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.ExtraPracticeResult)
                .WithMany(b => b.ExtraPracticeAnswers)
                .HasForeignKey(b => b.ExtraPracticeResultId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.SectionTimeCode)
               .WithMany(b => b.ExtraPracticeAnswers)
               .HasForeignKey(b => b.SectionTimeCodeId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.ExtraPracticeExerciseResult)
               .WithMany(b => b.ExtraPracticeAnswers)
               .HasForeignKey(b => b.ExtraPracticeExerciseResultId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.VideoTimeCode)
                      .WithMany(b => b.ExtraPracticeAnswers)
                      .HasForeignKey(b => b.VideoTimeCodeId)
                      .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Section)
              .WithMany(b => b.ExtraPracticeAnswers)
              .HasForeignKey(b => b.SectionId)
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.SectionGroupResult)
             .WithMany(b => b.ExtraPracticeAnswers)
             .HasForeignKey(b => b.SectionGroupResultId)
             .OnDelete(DeleteBehavior.NoAction);

            builder.Property(e => e.Status)
                   .HasMaxLength(20)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumAnswerStatus>());
        }
    }
}

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

    public class ExtraPracticeExerciseResultEntityTypeConfiguration : IEntityTypeConfiguration<ExtraPracticeExerciseResult>
    {
        public void Configure(EntityTypeBuilder<ExtraPracticeExerciseResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
               .HasMaxLength(20)
               .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumResultStatus>());

            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.CourseSkill)
               .HasMaxLength(20)
               .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumCourseSkill>());

            builder.HasOne(a => a.ExtraPracticeExercise)
                .WithMany(b => b.ExtraPracticeExerciseResults)
                .HasForeignKey(b => b.ExtraPracticeExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.ExtraPracticeResult)
              .WithMany(b => b.ExtraPracticeExerciseResults)
              .HasForeignKey(b => b.ExtraPracticeResultId)
              .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

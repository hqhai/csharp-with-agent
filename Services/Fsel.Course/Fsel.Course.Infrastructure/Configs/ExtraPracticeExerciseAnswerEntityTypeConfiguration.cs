// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ExtraPracticeExerciseAnswerEntityTypeConfiguration : IEntityTypeConfiguration<ExtraPracticeExerciseAnswer>
    {
        public void Configure(EntityTypeBuilder<ExtraPracticeExerciseAnswer> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.ExerciseQuestion)
                .WithMany(b => b.ExtraPracticeExerciseAnswers)
                .HasForeignKey(b => b.ExerciseQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.ExtraPracticeExercise)
                .WithMany(b => b.ExtraPracticeExerciseAnswers)
                .HasForeignKey(b => b.ExtraPracticeExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}

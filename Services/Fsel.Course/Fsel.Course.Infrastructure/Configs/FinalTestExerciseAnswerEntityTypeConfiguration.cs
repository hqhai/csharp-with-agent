// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class FinalTestExerciseAnswerEntityTypeConfiguration : IEntityTypeConfiguration<FinalTestExerciseAnswer>
    {
        public void Configure(EntityTypeBuilder<FinalTestExerciseAnswer> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.FinalTestResult)
                .WithMany(b => b.FinalTestExerciseAnswers)
                .HasForeignKey(b => b.FinalTestResultId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.ExerciseQuestion)
                .WithMany(b => b.FinalTestExerciseAnswers)
                .HasForeignKey(b => b.ExerciseQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

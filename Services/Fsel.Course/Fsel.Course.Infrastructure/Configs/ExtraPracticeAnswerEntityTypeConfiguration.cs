// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ExtraPracticeAnswerEntityTypeConfiguration : IEntityTypeConfiguration<ExtraPracticeAnswer>
    {
        public void Configure(EntityTypeBuilder<ExtraPracticeAnswer> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.ExerciseQuestion)
                .WithMany(b => b.ExtraPracticeAnswers)
                .HasForeignKey(b => b.ExerciseQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.ExtraPracticeResult)
                .WithMany(b => b.ExtraPracticeAnswers)
                .HasForeignKey(b => b.ExtraPracticeResultId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}

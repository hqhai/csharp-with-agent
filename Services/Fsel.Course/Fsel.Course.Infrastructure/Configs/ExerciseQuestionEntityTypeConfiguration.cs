// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class ExerciseQuestionEntityTypeConfiguration : IEntityTypeConfiguration<ExerciseQuestion>
    {
        public void Configure(EntityTypeBuilder<ExerciseQuestion> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.Exercise)
                .WithMany(b => b.ExerciseQuestions)
                .HasForeignKey(b => b.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Question)
                .WithMany(b => b.ExerciseQuestions)
                .HasForeignKey(b => b.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

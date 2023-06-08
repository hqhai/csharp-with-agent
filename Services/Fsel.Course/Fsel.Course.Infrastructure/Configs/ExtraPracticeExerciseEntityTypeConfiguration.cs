// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ExtraPracticeExerciseEntityTypeConfiguration : IEntityTypeConfiguration<ExtraPracticeExercise>
    {
        public void Configure(EntityTypeBuilder<ExtraPracticeExercise> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.ExtraPractice)
                .WithMany(b => b.ExtraPracticeExercises)
                .HasForeignKey(b => b.ExtraPracticeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.ExtraPracticeChapter)
                .WithMany(b => b.ExtraPracticeExercises)
                .HasForeignKey(b => b.ExtraPracticeChapterId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Exercise)
                .WithMany(b => b.ExtraPracticeExercises)
                .HasForeignKey(b => b.ExerciseId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

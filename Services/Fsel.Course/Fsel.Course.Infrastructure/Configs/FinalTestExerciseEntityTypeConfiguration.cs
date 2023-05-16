// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class FinalTestExerciseEntityTypeConfiguration : IEntityTypeConfiguration<FinalTestExercise>
    {
        public void Configure(EntityTypeBuilder<FinalTestExercise> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.FinalTest)
                .WithMany(b => b.FinalTestExercises)
                .HasForeignKey(b => b.FinalTestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Exercise)
                .WithMany(b => b.FinalTestExercises)
                .HasForeignKey(b => b.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

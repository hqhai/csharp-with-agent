// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class TimeCodeExerciseEntityTypeConfiguration : IEntityTypeConfiguration<TimeCodeExercise>
    {
        public void Configure(EntityTypeBuilder<TimeCodeExercise> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.VideoTimeCode)
                .WithMany(b => b.TimeCodeExercises)
                .HasForeignKey(b => b.VideoTimeCodeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Exercise)
                .WithMany(b => b.TimeCodeExercises)
                .HasForeignKey(b => b.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasAnnotation("SqlServer:RawSqlIndex",
                @"CREATE INDEX IX_TimeCodeExercises_IsDeleted_WithInclude
                ON [TimeCodeExercises] ([IsDeleted])
                INCLUDE ([ExerciseId], [VideoTimeCodeId])");
        }
    }
}

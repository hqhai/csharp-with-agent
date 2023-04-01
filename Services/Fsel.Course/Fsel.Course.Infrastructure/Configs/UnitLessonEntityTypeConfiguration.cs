// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class UnitLessonEntityTypeConfiguration : IEntityTypeConfiguration<UnitLesson>
    {
        public void Configure(EntityTypeBuilder<UnitLesson> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Lesson)
                .WithMany(b => b.UnitLessons)
                .HasForeignKey(b => b.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Unit)
                .WithMany(b => b.UnitLessons)
                .HasForeignKey(b => b.UnitId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class LessonStudentEntityTypeConfiguraion : IEntityTypeConfiguration<LessonStudent>
    {
        public void Configure(EntityTypeBuilder<LessonStudent> builder)
        {
            builder.HasOne(a => a.Unit)
                .WithMany(b => b.LessonStudents)
                .HasForeignKey(b => b.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Course)
                .WithMany(b => b.LessonStudents)
                .HasForeignKey(b => b.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Lesson)
                .WithMany(b => b.LessonStudents)
                .HasForeignKey(b => b.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

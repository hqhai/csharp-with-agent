// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Enums;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class LessonEntityTypeConfiguration : IEntityTypeConfiguration<Lesson>
    {
        public void Configure(EntityTypeBuilder<Lesson> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.CourseLevel)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseLevel>());

            builder.Property(e => e.VersionStatus)
                   .HasMaxLength(100)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumVersionStatus>());

            builder.Property(e => e.Status)
                   .HasMaxLength(100)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumStatus>());

            builder.HasOne(a => a.Level)
                   .WithMany(b => b.Lessons)
                   .HasForeignKey(p => p.LevelId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Category)
                   .WithMany(b => b.Lessons)
                   .HasForeignKey(p => p.ProgramId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

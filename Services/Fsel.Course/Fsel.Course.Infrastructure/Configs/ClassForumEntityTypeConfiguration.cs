// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class ClassForumEntityTypeConfiguration : IEntityTypeConfiguration<ClassForum>
    {
        public void Configure(EntityTypeBuilder<ClassForum> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.GradingStyle)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumGradingStyle>());
            builder.Property(e => e.CourseSkill)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseSkill>());
            builder.HasOne(a => a.Lesson)
                .WithOne(b => b.ClassForum)
                .HasForeignKey<ClassForum>(p => p.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.LessonId).IsUnique(false);
        }
    }
}

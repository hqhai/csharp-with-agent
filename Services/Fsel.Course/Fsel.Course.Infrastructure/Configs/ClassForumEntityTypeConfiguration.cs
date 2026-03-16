// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Enums;
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

            builder.Property(e => e.Layout)
                   .HasMaxLength(100)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumClassForumLayout>());

            builder.Property(e => e.CourseSkill)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseSkill>());

            builder.Property(e => e.VersionStatus)
                   .HasMaxLength(100)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumVersionStatus>());

            builder.HasOne(a => a.Lesson)
                .WithOne(b => b.ClassForum)
                .HasForeignKey<ClassForum>(p => p.LessonId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(a => a.Skill)
                .WithMany(b => b.ClassForums)
                .HasForeignKey(p => p.SkillId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Category)
                   .WithMany(b => b.ClassForums)
                   .HasForeignKey(x => x.ProgramId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

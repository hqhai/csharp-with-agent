// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class LessonInstructionEntityTypeConfiguration : IEntityTypeConfiguration<LessonInstruction>
    {
        public void Configure(EntityTypeBuilder<LessonInstruction> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Lesson)
                            .WithMany(b => b.LessonInstructions)
                            .HasForeignKey(b => b.LessonId)
                            .OnDelete(DeleteBehavior.Cascade);
            builder.Property(e => e.CourseSkill)
             .HasMaxLength(100)
             .HasConversion(
                 v => v.ToString(),
                 v => v.EnumParse<EnumCourseSkill>());

            builder.HasOne(a => a.Skill)
                .WithMany(b => b.LessonInstructions)
                .HasForeignKey(p => p.SkillId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

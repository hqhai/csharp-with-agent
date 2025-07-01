// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class SectionGroupEntityTypeConfiguration : IEntityTypeConfiguration<SectionGroup>
    {
        public void Configure(EntityTypeBuilder<SectionGroup> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.CourseSkill)
                     .HasMaxLength(100)
                     .HasConversion(
                         v => v.ToString(),
                         v => v.EnumParse<EnumCourseSkill>());

            builder.HasOne(a => a.Skill)
               .WithMany(b => b.SectionGroups)
               .HasForeignKey(p => p.SkillId)
               .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

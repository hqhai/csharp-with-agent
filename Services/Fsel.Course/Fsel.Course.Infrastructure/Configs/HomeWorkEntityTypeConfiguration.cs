// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Enums;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class HomeWorkEntityTypeConfiguration : IEntityTypeConfiguration<HomeWork>
    {
        public void Configure(EntityTypeBuilder<HomeWork> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.CourseLevel)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseLevel>());
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

            builder.HasOne(a => a.Skill)
                 .WithMany(b => b.HomeWorks)
                 .HasForeignKey(p => p.SkillId)
                 .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Program)
              .WithMany(b => b.HomeWorks)
              .HasForeignKey(p => p.ProgramId)
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Level)
             .WithMany(b => b.HomeWorks)
             .HasForeignKey(p => p.LevelId)
             .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

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
    public class HomeWorkEntityTypeConfiguration : IEntityTypeConfiguration<HomeWork>
    {
        public void Configure(EntityTypeBuilder<HomeWork> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.CourseLevel)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseLevel>());

            builder.Property(e => e.CourseSkill)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseSkill>());

            builder.Property(e => e.VersionStatus)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumVersionStatus>());

            builder.Property(e => e.VersionType)
              .HasMaxLength(20)
              .HasConversion(
                  v => v.ToString(),
                  v => v.EnumParse<EnumVersion>());

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

            builder.Property(e => e.Type)
               .HasMaxLength(20)
               .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumHomeWorkType>());

            builder.HasOne(a => a.Topic)
                   .WithMany(b => b.HomeWorks)
                   .HasForeignKey(b => b.TopicId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Enums;
using Fsel.Common.Helpers;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EntityCourse = Fsel.Course.Domain.Entities.Course;

namespace Fsel.Course.Infrastructure.Configs
{
    public class CourseEntityTypeConfiguration : IEntityTypeConfiguration<EntityCourse>
    {
        public void Configure(EntityTypeBuilder<EntityCourse> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.CourseLevel)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseLevel>());

            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.VersionStatus)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumVersionStatus>());

            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseStatus>());

            builder.HasOne(x => x.Level)
                   .WithMany(x => x.Courses)
                   .HasForeignKey(x => x.LevelId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Program)
                   .WithMany(x => x.Courses)
                   .HasForeignKey(x => x.ProgramId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(c => new { c.ParentCourseId, c.Priority }).IsUnique().HasFilter("ParentCourseId IS NOT NULL AND [IsDeleted] = 0");
        }
    }
}

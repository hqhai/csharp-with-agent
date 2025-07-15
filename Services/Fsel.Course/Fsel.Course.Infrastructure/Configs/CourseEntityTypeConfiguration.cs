// Copyright (c) Atlantic. All rights reserved.

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

            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseStatus>());

            builder.HasIndex(c => new { c.ParentCourseId, c.Priority }).IsUnique().HasFilter("ParentCourseId IS NOT NULL AND [IsDeleted] = 0");
        }
    }
}

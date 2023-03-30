// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System.Reflection.Emit;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CourseResultEntityTypeConfiguration : IEntityTypeConfiguration<CourseResult>
    {
        public void Configure(EntityTypeBuilder<CourseResult> builder)
        {
            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseStatus>());
            builder.HasOne(a => a.Course)
             .WithOne(b => b.CourseResult)
             .HasForeignKey<CourseResult>(p => p.CourseId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

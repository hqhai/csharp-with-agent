// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Enums;
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentEntityTypeConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.Property(e => e.CourseLevel)
                 .HasMaxLength(100)
                 .HasConversion(
                     v => v.ToString(),
                     v => (EnumCourseLevel)Enum.Parse(typeof(EnumCourseLevel), v));
            builder.HasOne(a => a.Human)
                    .WithOne(b => b.Student)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

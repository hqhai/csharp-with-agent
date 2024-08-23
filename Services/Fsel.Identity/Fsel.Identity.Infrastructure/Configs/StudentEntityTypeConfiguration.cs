// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentEntityTypeConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.CourseLevel)
                 .HasMaxLength(100)
                 .HasConversion(
                     v => v.ToString(),
                     v => v.EnumParse<EnumCourseLevel>());

            builder.Property(e => e.BaseCourseLevel)
                 .HasMaxLength(100)
                 .HasConversion(
                     v => v.ToString(),
                     v => v.EnumParse<EnumCourseLevel>());

            builder.HasOne(a => a.Human)
                    .WithOne(b => b.Student)
                    .HasForeignKey<Student>(b => b.HumanId)
                    .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => x.HumanId).IsUnique(false);
        }
    }
}

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

            builder.ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_Student_NumberOfToken_NonNegative", "[NumberOfToken] >= 0");
            });

            builder.HasIndex(x => x.HumanId).IsUnique(false);

            builder.HasIndex(x => new { x.IsDeleted, x.SchoolId });
            builder.HasIndex(x => new { x.IsDeleted }).IncludeValueProperties(x => new { x.CreatedDate, x.School, x.CourseLevel, x.HumanId, x.SchoolId });
            builder.HasIndexIncludeAllProperties(c => new { c.IsDeleted, c.SchoolId, c.SchoolClass });
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using RestSharp.Extensions;

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

            builder.HasOne(a => a.User)
                    .WithOne(b => b.Student)
                    .HasForeignKey<Student>(b => b.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.SchoolClassCampus)
                .WithMany(b => b.Students)
                .HasForeignKey(p => p.SchoolClassId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(e => e.StatusStudentCampus)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumStatusStudentCampus>());

            builder.ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_Student_NumberOfToken_NonNegative", "[NumberOfToken] >= 0");
            });

            builder.HasIndex(x => x.UserId).IsUnique(false);

            builder.HasIndex(x => new { x.IsDeleted, x.SchoolId });
            builder.HasIndex(x => new { x.IsDeleted }).IncludeValueProperties(x => new { x.CreatedDate, x.School, x.CourseLevel, x.SchoolId });
            builder.HasIndexIncludeAllProperties(c => new { c.IsDeleted, c.SchoolId, c.SchoolClass });
        }
    }
}

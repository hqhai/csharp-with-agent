// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Enums;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class HomeWorkEntityTypeConfiguration : IEntityTypeConfiguration<HomeWork>
    {
        public void Configure(EntityTypeBuilder<HomeWork> builder)
        {
            builder.Property(e => e.CourseLevel)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => (EnumCourseLevel)Enum.Parse(typeof(EnumCourseLevel), v));
            builder.Property(e => e.CourseSkill)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => (EnumCourseSkill)Enum.Parse(typeof(EnumCourseSkill), v));
        }
    }
}

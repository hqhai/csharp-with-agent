// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CourseTargetConfigEntityTypeConfigConfiguration : IEntityTypeConfiguration<CourseTargetConfig>
    {
        public void Configure(EntityTypeBuilder<CourseTargetConfig> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.CourseType)
                   .HasMaxLength(100)
                   .HasConversion(v => v.ToString(),
                   v => v.EnumParse<EnumCourseType>());
        }
    }
}

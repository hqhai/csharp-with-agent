// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums;
    using global::System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CourseSuggestConfigEntityTypeConfigConfiguration : IEntityTypeConfiguration<CourseSuggestConfig>
    {
        public void Configure(EntityTypeBuilder<CourseSuggestConfig> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.PlacementTestLevel)
                   .HasMaxLength(100)
                   .HasConversion(v => v.ToString(),
                   v => v.EnumParse<EnumCourseLevel>());

            builder.Property(e => e.Type)
                   .HasMaxLength(100)
                   .HasConversion(v => v.ToString(),
                   v => v.EnumParse<EnumCourseSuggestType>());
        }
    }
}

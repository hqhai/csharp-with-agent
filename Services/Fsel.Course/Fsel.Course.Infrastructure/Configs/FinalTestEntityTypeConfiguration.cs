// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class FinalTestEntityTypeConfiguration : IEntityTypeConfiguration<FinalTest>
    {
        public void Configure(EntityTypeBuilder<FinalTest> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.CourseType)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseType>());
        }
    }
}

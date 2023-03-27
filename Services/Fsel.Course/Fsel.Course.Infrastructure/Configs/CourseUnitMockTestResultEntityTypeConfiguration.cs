// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CourseUnitMockTestResultEntityTypeConfiguration : IEntityTypeConfiguration<CourseUnitMockTestResult>
    {
        public void Configure(EntityTypeBuilder<CourseUnitMockTestResult> builder)
        {
            builder.HasOne(a => a.CourseUnitMockTest)
                .WithMany(b => b.CourseUnitMockTestResults)
                .HasForeignKey(b => b.CourseUnitMockTestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumResultStatus>());

        }
    }
}

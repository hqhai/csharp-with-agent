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

    public class MockTestResultEntityTypeConfiguration : IEntityTypeConfiguration<MockTestResult>
    {
        public void Configure(EntityTypeBuilder<MockTestResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Course)
                .WithMany(b => b.MockTestResults)
                .HasForeignKey(b => b.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Unit)
                .WithMany(b => b.MockTestResults)
                .HasForeignKey(b => b.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.MockTest)
                .WithMany(b => b.MockTestResults)
                .HasForeignKey(b => b.MockTestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumResultStatus>());
        }
    }
}

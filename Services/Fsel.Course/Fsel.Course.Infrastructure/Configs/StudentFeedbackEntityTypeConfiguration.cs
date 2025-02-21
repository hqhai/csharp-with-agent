// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentFeedbackEntityTypeConfiguration : IEntityTypeConfiguration<StudentFeedback>
    {
        public void Configure(EntityTypeBuilder<StudentFeedback> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Type)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumStudentFeedBackType>());

            builder.Property(e => e.Feature)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumFeature>());

            builder.HasIndex(c => new { c.IsDeleted, c.Type, c.ObjectId });
            builder.HasIndex(c => new { c.IsDeleted, c.ObjectId });
        }
    }
}

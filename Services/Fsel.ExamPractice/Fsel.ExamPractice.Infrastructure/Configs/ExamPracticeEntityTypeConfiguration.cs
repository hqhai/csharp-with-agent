// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Enums;
    using Fsel.Common.Helpers;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ExamPracticeEntityTypeConfiguration : IEntityTypeConfiguration<ExamPractice>
    {
        public void Configure(EntityTypeBuilder<ExamPractice> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Type)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumExamPracticeType>());

            builder.Property(e => e.SubType)
                 .HasMaxLength(25)
                 .HasConversion(
                     v => v.ToString(),
                     v => v.EnumParse<EnumExamPracticeSubType>());

            builder.Property(e => e.Status)
                 .HasMaxLength(20)
                 .HasConversion(
                     v => v.ToString(),
                     v => v.EnumParse<EnumExamPracticeStatus>());

            builder.Property(e => e.CourseLevel)
                 .HasMaxLength(20)
                 .HasConversion(
                     v => v.ToString(),
                     v => (Shared.Enums.EnumCourseLevel?)v.EnumParse<EnumCourseLevel>());

            builder.Property(e => e.VersionStatus)
                   .HasMaxLength(100)
                   .HasConversion(v => v.ToString(), v => v.EnumParse<EnumVersionStatus>());
        }
    }
}

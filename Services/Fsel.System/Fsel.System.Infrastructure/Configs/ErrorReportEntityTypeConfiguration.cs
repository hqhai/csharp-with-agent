// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums;
    using global::System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ErrorReportEntityTypeConfiguration : IEntityTypeConfiguration<ErrorReport>
    {
        public void Configure(EntityTypeBuilder<ErrorReport> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.TypeOfError)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumTypeOfError>());

            builder.Property(e => e.Priority)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumPriority>());

            builder.Property(e => e.PlatFormDetail)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumPlatFormDetail>());

            builder.Property(e => e.LessonDetail)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumLessonDetail>());

            builder.Property(e => e.ReportStatus)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumErrorReportStatus>());
        }
    }
}

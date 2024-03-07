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

            builder.Property(e => e.Type)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumTypeOfError>());

            builder.Property(e => e.Priority)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumPriority>());

            builder.Property(e => e.FeaturePlatform)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumFeaturePlatform>());

            builder.Property(e => e.FeatureLearn)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumFeatureLearn>());

            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumErrorReportStatus>());
        }
    }
}

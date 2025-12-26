// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Domain.Entities;
    using Domain.Enums;
    using Fsel.Common.Enums;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using System;

    public class AICriteriaConfigsEntityTypeConfiguration : IEntityTypeConfiguration<AICriteriaConfigs>
    {
        public void Configure(EntityTypeBuilder<AICriteriaConfigs> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.FeatureMultiple)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumFeatureMultiple>());
            builder.Property(e => e.SubFeatureType)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumSubFeatureType>());
            builder.Property(e => e.TypeCriteriaAi)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCriteriaAi>());
            builder.Property(e => e.DefaultType)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumDefaultType>());

            builder.Property(e => e.VersionStatus)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumVersionStatus>());

            builder.Property(e => e.Version)
                .HasDefaultValue(1);

            builder.Property(e => e.VersionType)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumVersion>());

            builder.HasOne(e => e.AiPromptManager)
                .WithMany(m => m.AICriteriaConfigs)
                .HasForeignKey(e => e.AiPromptManagerId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

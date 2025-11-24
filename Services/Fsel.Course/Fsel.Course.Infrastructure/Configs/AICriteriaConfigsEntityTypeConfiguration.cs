// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Domain.Entities;
    using Domain.Enums;
    using Fsel.Common.Helpers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

            builder.HasOne(e => e.AiPromptManager)
                .WithMany(m => m.AICriteriaConfigs)
                .HasForeignKey(e => e.AiPromptManagerId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Domain.Entities;
    using Domain.Enums;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AiFeatureConfigEntityTypeConfiguration : IEntityTypeConfiguration<AIPromptConfigs>
    {
        public void Configure(EntityTypeBuilder<AIPromptConfigs> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.FeatureAi)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumFeature>());

            builder.Property(e => e.TypeFeatureAi)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumTypeFeatureAi>());

            builder.HasOne(x => x.AiPromptManager)
                .WithMany(x => x.AiModelFeatures)
                .HasForeignKey(x => x.AiPromptManagerId)
                .OnDelete(DeleteBehavior.NoAction);


            builder.HasOne(x => x.ParentFeature)
                .WithMany(x => x.SubFeatures)
                .HasForeignKey(x => x.ParentFeatureId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

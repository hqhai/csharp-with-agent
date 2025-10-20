// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Domain.Entities;
    using Domain.Enums;
    using Fsel.Common.Helpers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AiModelFeatureEntityTypeConfiguration : IEntityTypeConfiguration<AiModelFeature>
    {
        public void Configure(EntityTypeBuilder<AiModelFeature> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.FeatureAi)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumFeatureAi>());

            builder.Property(e => e.TypeFeatureAi)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumTypeFeatureAi>());

            builder.HasOne(x => x.AiModelManagers)
                .WithMany(x => x.AiModelFeatures)
                .HasForeignKey(x => x.AiModelManagerId)
                .OnDelete(DeleteBehavior.NoAction);


            builder.HasOne(x => x.ParentFeature)
                .WithMany(x => x.SubFeatures)
                .HasForeignKey(x => x.ParentFeatureId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

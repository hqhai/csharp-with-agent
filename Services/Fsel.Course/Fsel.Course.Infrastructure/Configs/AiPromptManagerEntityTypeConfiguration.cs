// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Domain.Entities;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AiPromptManagerEntityTypeConfiguration : IEntityTypeConfiguration<AiPromptManager>
    {
        public void Configure(EntityTypeBuilder<AiPromptManager> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.FeatureAi)
                    .HasMaxLength(100)
                    .HasConversion(
                        v => v.ToString(),
                        v => v.EnumParse<EnumFeature>());

            builder.HasOne(a => a.AiPromptParent)
                   .WithMany(b => b.AiPromptManagers)
                   .HasForeignKey(p => p.ParentId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

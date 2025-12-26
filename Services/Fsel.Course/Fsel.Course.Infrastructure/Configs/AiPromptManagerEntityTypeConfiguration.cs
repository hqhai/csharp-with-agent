// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Domain.Entities;
    using Fsel.Common.Enums;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AiPromptManagerEntityTypeConfiguration : IEntityTypeConfiguration<AiPromptManager>
    {
        public void Configure(EntityTypeBuilder<AiPromptManager> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            // Configure versioning properties
            builder.Property(e => e.VersionStatus)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(e => e.Version)
                .HasDefaultValue(1);


            builder.Property(e => e.VersionType)
                .HasConversion<string>()
                .HasMaxLength(10)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumVersion>());

            builder.HasOne(e => e.AiPromptParent)
                .WithMany(e => e.AiPromptChildren)
                .HasForeignKey(e => e.AiPromptManagerParentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

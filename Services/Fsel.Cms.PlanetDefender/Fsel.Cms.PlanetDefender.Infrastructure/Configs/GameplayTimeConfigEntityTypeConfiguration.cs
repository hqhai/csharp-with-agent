// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Configs
{
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class GameplayTimeConfigEntityTypeConfiguration : IEntityTypeConfiguration<GameplayTimeConfig>
    {
        public void Configure(EntityTypeBuilder<GameplayTimeConfig> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.GameVocabPDType)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumGameVocabPDType>());
        }
    }
}

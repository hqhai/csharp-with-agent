// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.System.Domain.Entities;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.Common.Enums;

    public class QuestBoardConfigConfigConfiguration : IEntityTypeConfiguration<QuestBoardConfig>
    {
        public void Configure(EntityTypeBuilder<QuestBoardConfig> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Type)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumQuestBoardType>());

            builder.Property(e => e.Category)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumQuestBoardCategory>());

            builder.Property(e => e.DisplayType)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumDisplayType>());

            builder.Property(e => e.Operator)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumFilterOperator>());
        }
    }
}

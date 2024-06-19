// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class GameVocabularyTypeEntityTypeConfiguration : IEntityTypeConfiguration<GameVocabularyType>
    {
        public void Configure(EntityTypeBuilder<GameVocabularyType> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.GameVocabType)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumGameVocabType>());
            builder.HasOne(a => a.GameVocabulary)
              .WithMany(b => b.GameVocabularyTypes)
              .HasForeignKey(b => b.GameVocabularyId)
              .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

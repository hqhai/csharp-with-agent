// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.System.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class GameVocabularyPlatformEntityTypeConfiguration : IEntityTypeConfiguration<GameVocabularyPlatform>
    {
        public void Configure(EntityTypeBuilder<GameVocabularyPlatform> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.GameVocabulary)
              .WithMany(b => b.GameVocabularyPlatforms)
              .HasForeignKey(b => b.GameVocabularyId)
              .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.System.Domain.Entities;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.EntityFrameworkCore;

    public class TokenHistoryTranslationEntityTypeConfiguration : IEntityTypeConfiguration<TokenHistoryTranslation>
    {
        public void Configure(EntityTypeBuilder<TokenHistoryTranslation> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.TokenHistory)
                   .WithMany(b => b.Translations)
                   .HasForeignKey(b => b.TokenHistoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

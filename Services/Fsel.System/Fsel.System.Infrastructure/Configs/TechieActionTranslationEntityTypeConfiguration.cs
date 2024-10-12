// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.System.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TechieActionTranslationEntityTypeConfiguration : IEntityTypeConfiguration<TechieActionTranslation>
    {
        public void Configure(EntityTypeBuilder<TechieActionTranslation> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.TechieAction)
                  .WithMany(b => b.Translations)
                  .HasForeignKey(b => b.TechieActionId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

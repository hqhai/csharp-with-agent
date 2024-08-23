// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Configs
{
    using System;
    using Fsel.Ordering.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class EventTranslationEntityTypeConfiguration : IEntityTypeConfiguration<EventTranslation>
    {
        public void Configure(EntityTypeBuilder<EventTranslation> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.Event)
                  .WithMany(b => b.Translations)
                  .HasForeignKey(b => b.EventId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

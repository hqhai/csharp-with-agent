// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CompetitionEventsEntityTypeConfiguration : IEntityTypeConfiguration<CompetitionEvent>
    {
        public void Configure(EntityTypeBuilder<CompetitionEvent> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.CompetitionEventParent)
                .WithMany(b => b.CompetitionEvents)
                .HasForeignKey(p => p.ParentEventId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

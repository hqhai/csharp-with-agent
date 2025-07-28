// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Shared.Enums;
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
                .OnDelete(DeleteBehavior.NoAction);

            builder.Property(e => e.Category)
               .HasMaxLength(100)
               .HasConversion(
                  v => v.HasValue ? v.ToString() : null,
                  v => v.EnumParse<EnumCompetitionEventCategory>());
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentRankingEventsEntityTypeConfiguration : IEntityTypeConfiguration<StudentRankingEvent>
    {
        public void Configure(EntityTypeBuilder<StudentRankingEvent> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.CompetitionEvents)
                .WithMany(b => b.StudentRankingEvents)
                .HasForeignKey(p => p.CompetitionEventId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

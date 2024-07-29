// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentRankingEventsEntityTypeConfiguration : IEntityTypeConfiguration<StudentRankingEvents>
    {
        public void Configure(EntityTypeBuilder<StudentRankingEvents> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(x => x.CompetitionEvents)
               .WithOne(b => b.StudentRankingEvents)
               .HasForeignKey<CompetitionEvents>(b => b.Id)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

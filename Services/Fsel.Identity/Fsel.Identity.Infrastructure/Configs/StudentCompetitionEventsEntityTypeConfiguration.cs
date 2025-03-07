// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentCompetitionEventsEntityTypeConfiguration : IEntityTypeConfiguration<StudentCompetitionEvent>
    {
        public void Configure(EntityTypeBuilder<StudentCompetitionEvent> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.CompetitionEvents)
                .WithMany(b => b.StudentCompetitionEvents)
                .HasForeignKey(p => p.CompetitionEventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => new { c.IsDeleted, c.StudentId });
            builder.HasIndex(c => new { c.StudentId }).IncludeValueProperties(x => new { x.CompetitionEventId });
            builder.HasIndex(c => new { c.CompetitionEventId }).IncludeValueProperties(x => new { x.StudentId });
        }
    }
}

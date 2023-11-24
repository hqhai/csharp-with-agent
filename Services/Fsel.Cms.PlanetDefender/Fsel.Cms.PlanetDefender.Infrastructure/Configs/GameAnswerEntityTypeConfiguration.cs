// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Configs
{
    using System;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class GameAnswerEntityTypeConfiguration : IEntityTypeConfiguration<GameAnswer>
    {
        public void Configure(EntityTypeBuilder<GameAnswer> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.GameHistory)
                  .WithMany(b => b.GameAnswers)
                  .HasForeignKey(b => b.GameHistoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

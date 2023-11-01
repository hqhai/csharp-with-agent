// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Configs
{
    using System;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class GameHistoryEntityTypeConfiguration : IEntityTypeConfiguration<GameHistory>
    {
        public void Configure(EntityTypeBuilder<GameHistory> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.SpaceShip)
                  .WithMany(b => b.GameHistories)
                  .HasForeignKey(b => b.SpaceShipId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

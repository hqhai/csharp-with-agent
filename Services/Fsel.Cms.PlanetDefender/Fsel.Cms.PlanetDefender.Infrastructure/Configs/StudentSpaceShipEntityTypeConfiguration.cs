// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Configs
{
    using System;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentSpaceShipEntityTypeConfiguration : IEntityTypeConfiguration<StudentSpaceShip>
    {
        public void Configure(EntityTypeBuilder<StudentSpaceShip> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.StudentGameInfo)
                  .WithMany(b => b.StudentSpaceShips)
                  .HasForeignKey(b => b.StudentGameInfoId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

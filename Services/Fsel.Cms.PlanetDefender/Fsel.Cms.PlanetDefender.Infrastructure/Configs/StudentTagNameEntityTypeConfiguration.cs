// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Configs
{
    using System;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentTagNameEntityTypeConfiguration : IEntityTypeConfiguration<StudentTagName>
    {
        public void Configure(EntityTypeBuilder<StudentTagName> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.SpaceShip)
                  .WithMany(b => b.StudentTagNames)
                  .HasForeignKey(b => b.MaxLevelSpaceShipId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

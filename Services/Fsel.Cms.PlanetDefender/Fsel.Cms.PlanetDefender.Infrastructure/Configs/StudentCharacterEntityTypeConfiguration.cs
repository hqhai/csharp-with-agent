// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Configs
{
    using System;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentCharacterEntityTypeConfiguration : IEntityTypeConfiguration<StudentCharacter>
    {
        public void Configure(EntityTypeBuilder<StudentCharacter> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.StudentGameInfo)
                  .WithMany(b => b.StudentCharacters)
                  .HasForeignKey(b => b.StudentGameInfoId)
                  .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(a => a.Character)
                  .WithMany(b => b.StudentCharacters)
                  .HasForeignKey(b => b.CharacterId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

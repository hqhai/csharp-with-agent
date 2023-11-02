// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Configs
{
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentGameAvatarEntityTypeConfiguration : IEntityTypeConfiguration<StudentGameAvatar>
    {
        public void Configure(EntityTypeBuilder<StudentGameAvatar> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.StudentGameInfo)
                  .WithMany(b => b.StudentGameAvatars)
                  .HasForeignKey(b => b.StudentGameInfoId)
                  .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.AvatarImage)
                  .WithMany(b => b.StudentGameAvatars)
                  .HasForeignKey(b => b.AvatarImageId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

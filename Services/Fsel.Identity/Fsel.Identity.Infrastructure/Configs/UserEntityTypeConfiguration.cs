// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(x => x.Human)
                .WithOne(b => b.User)
                .HasForeignKey<Human>(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Metadata.RemoveIndex(builder.HasIndex(u => u.NormalizedUserName).Metadata.Properties);
        }
    }
}

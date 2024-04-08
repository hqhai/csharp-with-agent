// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ParentEntityTypeConfiguration : IEntityTypeConfiguration<Parent>
    {
        public void Configure(EntityTypeBuilder<Parent> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Human)
                .WithOne(b => b.Parent)
                 .HasForeignKey<Parent>(b => b.HumanId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.HumanId).IsUnique(false);

            builder.HasOne(a => a.User)
                    .WithOne(b => b.Parent)
                    .HasForeignKey<Parent>(b => b.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            builder.HasIndex(x => x.UserId).IsUnique(false);
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CSOEntityTypeConfiguration : IEntityTypeConfiguration<CSO>
    {
        public void Configure(EntityTypeBuilder<CSO> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.User)
                    .WithOne(b => b.CSO)
                    .HasForeignKey<CSO>(b => b.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => x.UserId).IsUnique(false);
        }
    }
}

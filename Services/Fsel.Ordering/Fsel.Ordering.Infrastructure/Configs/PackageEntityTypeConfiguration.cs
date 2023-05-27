// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Configs
{
    using System;
    using Fsel.Ordering.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class PackageEntityTypeConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.Package)
                  .WithMany(b => b.Orders)
                  .HasForeignKey(b => b.PackageId)
                  .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class PackageEventEntityTypeConfiguration : IEntityTypeConfiguration<PackageEvent>
    {
        public void Configure(EntityTypeBuilder<PackageEvent> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.Package)
                  .WithMany(b => b.PackageEvents)
                  .HasForeignKey(b => b.PackageId)
                  .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(a => a.Event)
                  .WithMany(b => b.PackageEvents)
                  .HasForeignKey(b => b.EventId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

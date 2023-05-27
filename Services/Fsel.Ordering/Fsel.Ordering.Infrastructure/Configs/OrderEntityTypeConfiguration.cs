// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.Package)
                  .WithMany(b => b.Orders)
                  .HasForeignKey(b => b.PackageId)
                  .OnDelete(DeleteBehavior.NoAction);

            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumOrderStatus>());

            builder.Property(e => e.PaymentMethod)
               .HasMaxLength(100)
               .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumPaymentMethodStatus>());
        }
    }
}

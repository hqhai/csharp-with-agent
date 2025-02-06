// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Shared.Enums;
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
                  .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Event)
                  .WithMany(b => b.Orders)
                  .HasForeignKey(b => b.EventId)
                  .OnDelete(DeleteBehavior.NoAction);

            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumOrderStatus>());

            builder.Property(e => e.PaymentMethod)
               .HasMaxLength(100)
               .HasConversion(
                   v => v == null ? null : v.ToString(),
                   v => string.IsNullOrEmpty(v) ? null : v.EnumParse<EnumPaymentMethodStatus>());

            builder.Property(e => e.RevenueType)
               .HasMaxLength(100)
               .HasConversion(
                   v => v == null ? null : v.ToString(),
                   v => string.IsNullOrEmpty(v) ? null : v.EnumParse<EnumPaymentRevenueType>());

            builder.HasIndex(c => new { c.IsDeleted, c.Status, c.UserId, c.IsTrial });
            builder.HasIndex(c => new { c.IsDeleted, c.UserId });
            builder.HasIndex(c => new { c.IsDeleted, c.Code });
        }
    }
}

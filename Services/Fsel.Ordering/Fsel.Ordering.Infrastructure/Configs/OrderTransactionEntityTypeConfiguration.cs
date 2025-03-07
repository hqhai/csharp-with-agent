// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class OrderTransactionEntityTypeConfiguration : IEntityTypeConfiguration<OrderTransaction>
    {
        public void Configure(EntityTypeBuilder<OrderTransaction> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumOrderTransactionStatus>());
            builder.Property(e => e.Type)
               .HasMaxLength(100)
               .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumOrderTransactionType>());
            builder.HasOne(a => a.Order)
                  .WithMany(b => b.OrderTransactions)
                  .HasForeignKey(b => b.OrderId)
                  .OnDelete(DeleteBehavior.ClientCascade);

            builder.HasOne(a => a.Product)
               .WithMany(b => b.OrderTransactions)
               .HasForeignKey(b => b.ProductId)
               .OnDelete(DeleteBehavior.ClientCascade);

            builder.HasIndex(c => new { c.CreatedUserId, c.IsDeleted, c.Status, c.Type });
        }
    }
}

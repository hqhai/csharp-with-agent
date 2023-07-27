// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Configs
{
    using Fsel.Ordering.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UserVoucherEnityTypeConfiguration : IEntityTypeConfiguration<UserVoucher>
    {
        public void Configure(EntityTypeBuilder<UserVoucher> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.Voucher)
                  .WithMany(b => b.UserVouchers)
                  .HasForeignKey(b => b.VoucherId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Configs
{
    using Fsel.Ordering.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class VoucherPackageEntityTypeConfiguration : IEntityTypeConfiguration<VoucherPackage>
    {
        public void Configure(EntityTypeBuilder<VoucherPackage> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.Package)
               .WithMany(b => b.VoucherPackages)
               .HasForeignKey(b => b.PackageId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Voucher)
                .WithMany(b => b.VoucherPackages)
                .HasForeignKey(b => b.VoucherId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

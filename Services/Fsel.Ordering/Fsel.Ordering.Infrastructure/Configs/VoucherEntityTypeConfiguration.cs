// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Configs
{
    using System;
    using Fsel.Ordering.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class VoucherEntityTypeConfiguration : IEntityTypeConfiguration<Voucher>
    {
        public void Configure(EntityTypeBuilder<Voucher> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
        }
    }
}

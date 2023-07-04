// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Configs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class VoucherEntityTypeConfiguration : IEntityTypeConfiguration<Voucher>
    {
        public void Configure(EntityTypeBuilder<Voucher> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.CustomerType)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCustomerType>());

            builder.Property(e => e.CourseLevel)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseLevel>());
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs.Crm
{
    using Fsel.Common.Helpers;
    using Fsel.System.Domain.Entities;
    using global::System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CrmLocationEntityTypeConfigConfiguration : IEntityTypeConfiguration<CrmLocation>
    {
        public void Configure(EntityTypeBuilder<CrmLocation> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            var building = "building";
            var site = "site";

            builder.Property(e => e.TypeName)
                .HasMaxLength(100)
                .HasConversion(
                    v => v == EnumCrmLocationTypeName.School ? building : site,
                    v => v == building ? EnumCrmLocationTypeName.School : EnumCrmLocationTypeName.Site);
        }
    }
}

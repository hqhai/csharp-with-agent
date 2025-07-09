// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Domain.Enums;
    using Fsel.Common.Helpers;
    using Domain.Entities.V1i1;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UnitModuleEntityTypeConfiguration : IEntityTypeConfiguration<UnitModule>
    {
        public void Configure(EntityTypeBuilder<UnitModule> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.UnitConfigType)
                   .HasMaxLength(20)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumUnitConfigType>());

            builder.HasOne(a => a.Unit)
                .WithMany(b => b.UnitModules)
                .HasForeignKey(a => a.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(u => u.OriginalId);
        }
    }
}

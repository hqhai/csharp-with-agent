// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using global::System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class LocationEntityTypeConfigConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Type)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumLocationType>());
            builder.HasOne(l => l.Parent).WithMany(l => l.Children).HasForeignKey(l => l.ParentId).OnDelete(DeleteBehavior.Restrict);

            builder.Property(e => e.IsActive).HasDefaultValue(true);

            builder.Property(l => l.Name)
               .UseCollation(CollationSetting.SQLLatin1GeneralCP1CIAI); // Set the collation
        }
    }
}

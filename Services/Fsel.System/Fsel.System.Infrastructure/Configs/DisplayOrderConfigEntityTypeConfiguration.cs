// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class DisplayOrderConfigEntityTypeConfiguration : IEntityTypeConfiguration<DisplayOrderConfig>
    {
        public void Configure(EntityTypeBuilder<DisplayOrderConfig> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);


            builder.Property(e => e.Name)
                   .HasMaxLength(100)
                   .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumDisplayOrder>());
        }
    }
}

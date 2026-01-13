// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using global::System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class SenderConfigEntityTypeConfiguration : IEntityTypeConfiguration<SenderConfig>
    {
        public void Configure(EntityTypeBuilder<SenderConfig> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.Type)
                   .HasMaxLength(100)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumSenderConfigType>());
        }
    }
}

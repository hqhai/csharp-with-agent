// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Sender.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class MessageHistoryEntityTypeConfiguration : IEntityTypeConfiguration<MessageHistory>
    {
        public void Configure(EntityTypeBuilder<MessageHistory> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Type)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumMessageHistoryType>());
            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumMessageHistoryStatus>());
        }
    }
}

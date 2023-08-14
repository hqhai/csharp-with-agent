// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class SupportTicketEntityTypeConfiguration : IEntityTypeConfiguration<SupportTicket>
    {
        public void Configure(EntityTypeBuilder<SupportTicket> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumSupportTicketStatus>());

            builder.HasOne(a => a.SupportCategory)
                .WithMany(b => b.SupportTickets)
                .HasForeignKey(b => b.SupportCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(a => a.SupportQuestion)
                .WithMany(b => b.SupportTickets)
                .HasForeignKey(b => b.SupportQuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}

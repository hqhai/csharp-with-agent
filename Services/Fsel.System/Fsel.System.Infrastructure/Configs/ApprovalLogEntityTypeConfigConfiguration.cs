// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using global::System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ApprovalLogEntityTypeConfigConfiguration : IEntityTypeConfiguration<ApprovalLog>
    {
        public void Configure(EntityTypeBuilder<ApprovalLog> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumApprovalLogStatus>());

            builder.HasOne(a => a.ApprovalTimeConfig)
                   .WithMany(b => b.ApprovalLogs)
                   .HasForeignKey(b => b.ApprovalTimeConfigId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

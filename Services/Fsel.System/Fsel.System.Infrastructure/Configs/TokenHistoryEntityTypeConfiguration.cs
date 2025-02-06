// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TokenHistoryEntityTypeConfiguration : IEntityTypeConfiguration<TokenHistory>
    {
        public void Configure(EntityTypeBuilder<TokenHistory> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Type)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumTokenHistoryType>());

            builder.HasOne(a => a.TokenConfig)
                   .WithMany(b => b.TokenHistories)
                   .HasForeignKey(b => b.TokenConfigId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.Feature)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumTokenFeature>());

            builder.Property(e => e.Mission)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumTokenMission>());

            builder.HasIndex(c => new { c.IsDeleted, c.UserId, c.CreatedDate });
            builder.HasIndex(c => new { c.IsDeleted, c.UserId, c.Type });
        }
    }
}

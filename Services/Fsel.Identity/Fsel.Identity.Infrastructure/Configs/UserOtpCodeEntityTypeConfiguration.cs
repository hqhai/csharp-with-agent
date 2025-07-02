// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UserOtpCodeEntityTypeConfiguration : IEntityTypeConfiguration<UserOtpCode>
    {
        public void Configure(EntityTypeBuilder<UserOtpCode> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.User)
                 .WithMany(b => b.UserOtpCodes)
                 .HasForeignKey(p => p.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.UserId).IsUnique(false);

            builder.Property(e => e.Status)
                   .HasMaxLength(100)
                   .HasConversion(
                        v => v.ToString(),
                        v => v.EnumParse<EnumOtpCodeStatus>());

            builder.Property(e => e.Type)
                   .HasMaxLength(100)
                   .HasConversion(
                        v => v.ToString(),
                        v => v.EnumParse<EnumUserOtpCodeType>());

            builder.HasIndex(x => new { x.Status });
            builder.HasIndex(x => new { x.UserId, x.Status });
            builder.HasIndex(x => new { x.UserId, x.OtpCode });
            builder.HasIndex(x => new { x.IsDeleted, x.OtpCode, x.Status });
        }
    }
}

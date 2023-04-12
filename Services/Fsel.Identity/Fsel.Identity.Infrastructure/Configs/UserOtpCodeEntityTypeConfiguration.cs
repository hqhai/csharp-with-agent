// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UserOtpCodeEntityTypeConfiguration : IEntityTypeConfiguration<UserOtpCode>
    {
        public void Configure(EntityTypeBuilder<UserOtpCode> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(x => x.User).WithOne(b => b.UserOtpCode)
                        .HasForeignKey<UserOtpCode>(b => b.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.UserId).IsUnique(true);

            builder.Property(e => e.Status)
                   .HasMaxLength(100)
                   .HasConversion(
                        v => v.ToString(),
                        v => v.EnumParse<EnumStatusUser>());
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UserOtpEntityTypeConfiguration : IEntityTypeConfiguration<UserOtp>
    {
        public void Configure(EntityTypeBuilder<UserOtp> builder)
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
                        v => v.EnumParse<EnumUserOtpStatus>());
        }
    }
}

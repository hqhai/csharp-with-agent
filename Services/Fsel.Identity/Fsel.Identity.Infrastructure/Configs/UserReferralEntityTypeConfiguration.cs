// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UserReferralEntityTypeConfiguration : IEntityTypeConfiguration<UserReferral>
    {
        public void Configure(EntityTypeBuilder<UserReferral> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Sender)
                 .WithMany(b => b.Senders)
                 .HasForeignKey(p => p.SenderId)
                 .OnDelete(DeleteBehavior.NoAction);

            builder.Property(e => e.Type)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumUserReferralType>());
        }
    }
}

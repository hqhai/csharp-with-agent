// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UserDeletionEntityTypeConfiguration : IEntityTypeConfiguration<UserDeletion>
    {
        public void Configure(EntityTypeBuilder<UserDeletion> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.User)
                 .WithMany(b => b.UserDeletions)
                 .HasForeignKey(p => p.UserId)
                 .OnDelete(DeleteBehavior.NoAction);

            builder.Property(e => e.Status)
                   .HasMaxLength(100)
                   .HasConversion(
                        v => v.ToString(),
                        v => v.EnumParse<EnumUserDeletionStatus>());

            builder.Property(e => e.Reason)
                   .HasMaxLength(100)
                   .HasConversion(
                        v => v.ToString(),
                        v => v.EnumParse<EnumUserDeletionReason>());
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(x => x.Human)
                .WithOne(b => b.User)
                .HasForeignKey<Human>(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //builder.Metadata.RemoveIndex(builder.HasIndex(u => u.NormalizedUserName).Metadata.Properties);

            builder.HasIndex(x => new { x.IsDeleted, x.Email });

            builder.Property(e => e.Status)
                 .HasMaxLength(100)
                 .HasConversion(
                    v => v.HasValue ? v.ToString() : null,
                    v => v.EnumParse<EnumUserStatus>());
        }
    }
}

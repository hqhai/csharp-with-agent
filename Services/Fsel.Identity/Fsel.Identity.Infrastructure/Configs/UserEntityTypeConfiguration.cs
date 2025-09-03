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

            builder.Property(e => e.Gender)
                 .HasMaxLength(100)
                 .HasConversion(
                    v => v.HasValue ? v.ToString() : null,
                    v => v.EnumParse<EnumGender>());

            builder.Property(e => e.Status)
                 .HasMaxLength(100)
                 .HasConversion(
                    v => v.HasValue ? v.ToString() : null,
                    v => v.EnumParse<EnumUserStatus>());

            //builder.Metadata.RemoveIndex(builder.HasIndex(u => u.NormalizedUserName).Metadata.Properties);
            builder.HasIndex(x => x.NormalizedUserName)
                .HasFilter("[NormalizedUserName] IS NOT NULL AND [IsDeleted] = 0");

            builder.Property(x => x.FullName)
                .HasComputedColumnSql($"CONCAT_WS(' ', [{nameof(User.LastName)}], [{nameof(User.FirstName)}])", stored: true);

            builder.HasIndex(x => new { x.IsDeleted, x.UserName });
            builder.HasIndex(x => new { x.IsDeleted, x.Email });
            builder.HasIndex(x => new { x.IsDeleted, x.PhoneNumber });
            builder.HasIndex(x => new { x.IsDeleted, x.Id, x.UserName, x.Email });
            builder.HasIndex(x => new { x.Id, x.ConcurrencyStamp });
        }
    }
}

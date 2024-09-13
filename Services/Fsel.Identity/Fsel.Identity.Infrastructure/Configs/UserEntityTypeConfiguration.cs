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

            //builder.Metadata.RemoveIndex(builder.HasIndex(u => u.NormalizedUserName).Metadata.Properties);
        }
    }
}

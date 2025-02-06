// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.System.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UserConfigEntityTypeConfiguration : IEntityTypeConfiguration<UserConfig>
    {
        public void Configure(EntityTypeBuilder<UserConfig> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasIndex(c => new { c.IsDeleted, c.UserId, c.IsViewNewFeature });
        }
    }
}

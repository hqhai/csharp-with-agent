// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.System.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class FocusTimeConfigEntityTypeConfiguration : IEntityTypeConfiguration<FocusTimeConfig>
    {
        public void Configure(EntityTypeBuilder<FocusTimeConfig> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.System.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class FselRatingEntityTypeConfiguration : IEntityTypeConfiguration<FselRating>
    {
        public void Configure(EntityTypeBuilder<FselRating> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
        }
    }
}

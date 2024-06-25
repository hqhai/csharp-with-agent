// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class ProsodyScoreEntityTypeConfiguration : IEntityTypeConfiguration<ProsodyScore>
    {
        public void Configure(EntityTypeBuilder<ProsodyScore> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
        }
    }
}

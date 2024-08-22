// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.System.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TechieEntityTypeConfiguration : IEntityTypeConfiguration<Techie>
    {
        public void Configure(EntityTypeBuilder<Techie> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
        }
    }
}

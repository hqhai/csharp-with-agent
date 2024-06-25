// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Configs
{
    using System;
    using Fsel.Ordering.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class PackageTranslationEntityTypeConfiguration : IEntityTypeConfiguration<PackageTranslation>
    {
        public void Configure(EntityTypeBuilder<PackageTranslation> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.Package)
                  .WithMany(b => b.Translations)
                  .HasForeignKey(b => b.PackageId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

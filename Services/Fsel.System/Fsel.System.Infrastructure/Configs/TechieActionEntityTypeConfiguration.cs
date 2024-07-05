// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TechieActionEntityTypeConfiguration : IEntityTypeConfiguration<TechieAction>
    {
        public void Configure(EntityTypeBuilder<TechieAction> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Action)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumTechieAction>());

            builder.Property(e => e.Feature)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumTechieFeature>());

            builder.HasOne(a => a.Techie)
                   .WithMany(b => b.TechieActions)
                   .HasForeignKey(b => b.TechieId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

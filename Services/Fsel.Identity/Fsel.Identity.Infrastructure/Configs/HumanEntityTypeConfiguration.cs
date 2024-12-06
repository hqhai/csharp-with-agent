// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class HumanEntityTypeConfiguration : IEntityTypeConfiguration<Human>
    {
        public void Configure(EntityTypeBuilder<Human> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(x => x.User)
                .WithOne(b => b.Human)
                .HasForeignKey<Human>(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.UserId).IsUnique(false);

            builder.Property(e => e.Gender)
                 .HasMaxLength(100)
                 .HasConversion(
                    v => v.HasValue ? v.ToString() : null,
                    v => v.EnumParse<EnumGender>());

            builder.HasIndex(x => new { x.IsDeleted, x.Code });
        }
    }
}

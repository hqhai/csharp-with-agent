// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CategoryEntityTypeConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.Type)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumTypeCategory>());

            builder.Property(e => e.TestMode)
              .HasMaxLength(20)
              .HasConversion(
                  v => v.ToString(),
                  v => v.EnumParse<EnumTestMode>());

            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumStatus>());

            builder.HasOne(a => a.CategoryParent)
                   .WithMany(b => b.Categorys)
                   .HasForeignKey(p => p.ParentId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

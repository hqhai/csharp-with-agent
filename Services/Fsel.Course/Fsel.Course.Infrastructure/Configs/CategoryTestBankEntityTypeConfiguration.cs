// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CategoryTestBankEntityTypeConfiguration : IEntityTypeConfiguration<CategoryTestBank>
    {
        public void Configure(EntityTypeBuilder<CategoryTestBank> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.TestType)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumTestType>());

            builder.HasOne(a => a.Category)
                   .WithMany(b => b.CategoryTestBanks)
                   .HasForeignKey(p => p.ProgramId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

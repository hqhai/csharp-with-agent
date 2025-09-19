// Copyright (c) Atlantic. All rights reserved.
using Fsel.Common.Helpers;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EntityRubyText = Fsel.Course.Domain.Entities.RubyText;

namespace Fsel.Course.Infrastructure.Configs
{
    public class RubyTextTypeConfiguration : IEntityTypeConfiguration<EntityRubyText>
    {
        public void Configure(EntityTypeBuilder<EntityRubyText> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.LanguageType)
                .HasMaxLength(50)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumLanguageType>());

            builder.HasIndex(c => new { c.BaseText }).IsUnique().HasFilter("BaseText IS NOT NULL AND [IsDeleted] = 0");
        }
    }
}

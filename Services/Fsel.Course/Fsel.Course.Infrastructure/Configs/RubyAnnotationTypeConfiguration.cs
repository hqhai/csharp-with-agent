// Copyright (c) Atlantic. All rights reserved.
using Fsel.Common.Helpers;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EntityRubyText = Fsel.Course.Domain.Entities.RubyAnnotation;

namespace Fsel.Course.Infrastructure.Configs
{
    public class RubyAnnotationTypeConfiguration : IEntityTypeConfiguration<EntityRubyText>
    {
        public void Configure(EntityTypeBuilder<EntityRubyText> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.LanguageType)
                .HasMaxLength(50)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumLanguageType>());
            builder.Property(e => e.TextNote)
                .HasMaxLength(100);
            builder.Property(e => e.SelectedText)
                .HasMaxLength(50);
            builder.HasOne(e => e.Scope).WithMany(s => s.RubyAnnotations)
                .HasForeignKey(e => e.RubyScopeId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(e => new {e.RubyScopeId, e.StartGrapheme});
            builder.HasIndex(e => new { e.RubyScopeId, e.SelectedText});
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class KeyboardTextEntityTypeConfiguration : IEntityTypeConfiguration<KeyboardText>
    {
        public void Configure(EntityTypeBuilder<KeyboardText> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(x => x.KeyboardLayout)
                   .WithMany(x => x.KeyboardTexts)
                   .HasForeignKey(x => x.KeyboardLayoutId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.VectorLibrary)
                  .HasMaxLength(20)
                  .HasConversion(
                      v => v.ToString(),
                      v => v.EnumParse<EnumVectorLibrary>());
        }
    }
}

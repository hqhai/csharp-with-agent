// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
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
        }
    }
}

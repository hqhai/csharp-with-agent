// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CommentEntityTypeConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Type)
              .HasMaxLength(100)
              .HasConversion(
                  v => v.ToString(),
                  v => v.EnumParse<EnumInteractionType>());
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Configs
{
    using Fsel.Interaction.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class PostTagEntityTypeConfiguration : IEntityTypeConfiguration<PostTag>
    {
        public void Configure(EntityTypeBuilder<PostTag> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Post)
                  .WithMany(b => b.PostTags)
                  .HasForeignKey(b => b.PostId)
                  .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.TopicTag)
                  .WithMany(b => b.PostTags)
                  .HasForeignKey(b => b.TopicTagId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

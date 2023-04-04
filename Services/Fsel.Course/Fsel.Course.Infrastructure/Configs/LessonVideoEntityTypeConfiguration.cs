// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class LessonVideoEntityTypeConfiguration : IEntityTypeConfiguration<LessonVideo>
    {
        public void Configure(EntityTypeBuilder<LessonVideo> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Lesson)
                .WithMany(b => b.LessonVideos)
                .HasForeignKey(b => b.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(a => a.Video)
                .WithMany(b => b.LessonVideos)
                .HasForeignKey(b => b.VideoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

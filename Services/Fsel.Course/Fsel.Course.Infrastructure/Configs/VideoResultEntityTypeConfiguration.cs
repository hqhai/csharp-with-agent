// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class VideoResultEntityTypeConfiguration : IEntityTypeConfiguration<VideoResult>
    {
        public void Configure(EntityTypeBuilder<VideoResult> builder)
        {
            builder.HasOne(a => a.Video)
              .WithMany(b => b.VideoResults)
              .HasForeignKey(b => b.VideoId)
              .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.LessonResult)
                .WithOne(b => b.VideoResult)
                .HasForeignKey<VideoResult>(p => p.LessonResultId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.LessonResultId).IsUnique(false);

            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumResultStatus>());
        }
    }
}

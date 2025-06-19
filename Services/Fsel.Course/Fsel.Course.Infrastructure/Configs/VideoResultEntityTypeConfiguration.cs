// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
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
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumResultStatus>());

            builder.Property(e => e.PlaybackSpeed)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumPlaybackSpeed>());

            builder.HasIndex(c => new { c.LessonResultId, c.VideoId, c.StudentId }).IsUnique().HasFilter("[IsDeleted] = 0");
            builder.HasIndex(c => new { c.Status, c.StudentId });
            builder.HasIndex(c => new { c.StudentId });
        }
    }
}

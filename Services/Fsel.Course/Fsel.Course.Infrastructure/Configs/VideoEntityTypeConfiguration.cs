// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Enums;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class VideoEntityTypeConfiguration : IEntityTypeConfiguration<Video>
    {
        public void Configure(EntityTypeBuilder<Video> builder)
        {
            builder.Property(e => e.CourseLevel)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseLevel>());

            builder.Property(e => e.Type)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumVideoType>());

            builder.Property(e => e.VersionStatus)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumVersionStatus>());

            builder.HasOne(a => a.OriginalVideo)
                   .WithMany(b => b.Videos)
                   .HasForeignKey(p => p.OriginalId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Program)
                   .WithMany(b => b.Videos)
                   .HasForeignKey(p => p.ProgramId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Level)
                   .WithMany(b => b.Videos)
                   .HasForeignKey(p => p.LevelId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

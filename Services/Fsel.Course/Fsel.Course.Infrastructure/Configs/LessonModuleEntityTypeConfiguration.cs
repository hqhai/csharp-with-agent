// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class LessonModuleEntityTypeConfiguration : IEntityTypeConfiguration<LessonModule>
    {
        public void Configure(EntityTypeBuilder<LessonModule> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.LessonConfigType)
                   .HasMaxLength(20)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumLessonConfigType>());

            builder.HasOne(a => a.Lesson)
                   .WithMany(b => b.LessonModules)
                   .HasForeignKey(p => p.LessonId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Video)
                   .WithMany(b => b.LessonModules)
                   .HasForeignKey(p => p.VideoId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.ClassForum)
                   .WithMany(b => b.LessonModules)
                   .HasForeignKey(p => p.ClassForumId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.HomeWork)
                   .WithMany(b => b.LessonModules)
                   .HasForeignKey(p => p.HomeWorkId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Document)
                   .WithMany(b => b.LessonModules)
                   .HasForeignKey(p => p.DocumentId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

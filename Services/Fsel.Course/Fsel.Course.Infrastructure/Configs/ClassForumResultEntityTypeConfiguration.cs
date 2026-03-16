// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ClassForumResultEntityTypeConfiguration : IEntityTypeConfiguration<ClassForumResult>
    {
        public void Configure(EntityTypeBuilder<ClassForumResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumClassForumResultStatus>());

            builder.Property(e => e.ResultStatus)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumResultStatus>());

            builder.Property(e => e.SubmissionCount)
               .HasMaxLength(20)
               .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumSubmissionCount>());

            builder.HasOne(a => a.ClassForum)
                 .WithMany(b => b.ClassForumResults)
                 .HasForeignKey(p => p.ClassForumId)
                 .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.LessonResult)
                 .WithMany(b => b.ClassForumResults)
                 .HasForeignKey(p => p.LessonResultId)
                 .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(a => a.LessonModule)
                .WithMany(b => b.ClassForumResults)
                .HasForeignKey(p => p.LessonModuleId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(c => new { c.IsDeleted, c.StudentId });
            builder.HasIndex(x => new { x.IsDeleted, x.Status, x.ClassForumId, x.Id }).IncludeValueProperties(x => new { x.StudentId });
            builder.HasIndex(c => new { c.LessonResultId, c.LessonModuleId, c.IsDeleted });
        }
    }
}

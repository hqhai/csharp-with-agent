// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ClassForumResultEntityTypeConfiguration : IEntityTypeConfiguration<ClassForumResult>
    {
        public void Configure(EntityTypeBuilder<ClassForumResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumClassForumResultStatus>());

            builder.HasOne(a => a.ClassForum)
                 .WithMany(b => b.ClassForumResults)
                 .HasForeignKey(p => p.ClassForumId)
                 .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(a => a.LessonResult)
                 .WithMany(b => b.ClassForumResults)
                 .HasForeignKey(p => p.LessonResultId)
                 .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}

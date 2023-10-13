// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ClassForumResultFlagEntityTypeConfiguration : IEntityTypeConfiguration<ClassForumResultFlag>
    {
        public void Configure(EntityTypeBuilder<ClassForumResultFlag> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.ClassForumResult)
                 .WithMany(b => b.ClassForumResultFlags)
                 .HasForeignKey(p => p.ClassForumResultId)
                 .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumClassForumResultFlagStatus>());

            builder.Property(e => e.FlagIssue)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumClassForumResultFlagIssue>());
        }
    }
}

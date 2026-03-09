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

    public class ClassForumDetailResultTypeConfiguration : IEntityTypeConfiguration<ClassForumDetailResult>
    {
        public void Configure(EntityTypeBuilder<ClassForumDetailResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.ClassForumResult)
                 .WithMany(b => b.ClassForumDetailResults)
                 .HasForeignKey(p => p.ClassForumResultId)
                 .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.SubmissionCount)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumSubmissionCount>());

            builder.Property(e => e.Status)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumClassForumResultStatus>());

            builder.HasIndex(c => new { c.IsDeleted, c.ClassForumResultId });
            builder.HasIndex(c => new { c.IsDeleted, c.Status, c.ClassForumResultId });
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ClassForumScoreEntityTypeConfiguration : IEntityTypeConfiguration<ClassForumScore>
    {
        public void Configure(EntityTypeBuilder<ClassForumScore> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Criteria)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumClassForumScoreCriteria>());

            builder.HasOne(a => a.ClassForumResult)
                 .WithMany(b => b.ClassForumScores)
                 .HasForeignKey(p => p.ClassForumResultId)
                 .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

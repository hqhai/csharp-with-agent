// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class MockTestScoreEntityTypeConfiguration : IEntityTypeConfiguration<MockTestScore>
    {
        public void Configure(EntityTypeBuilder<MockTestScore> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Criteria)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumClassForumScoreCriteria>());

            builder.HasOne(a => a.MockTestResult)
                 .WithMany(b => b.MockTestScores)
                 .HasForeignKey(p => p.MockTestResultId)
                 .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.SectionGroup)
                .WithMany(b => b.MockTestScores)
                .HasForeignKey(p => p.MockTestResultId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

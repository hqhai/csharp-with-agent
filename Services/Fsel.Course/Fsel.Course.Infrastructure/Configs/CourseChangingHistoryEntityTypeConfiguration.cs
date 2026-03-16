// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CourseChangingHistoryEntityTypeConfiguration : IEntityTypeConfiguration<CourseChangingHistory>
    {
        public void Configure(EntityTypeBuilder<CourseChangingHistory> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.ToCourseResult)
                .WithMany(b => b.CourseChangingHistories)
                .HasForeignKey(p => p.ToCourseResultId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.PtTestResult)
                .WithMany(b => b.CourseChangingHistories)
                .HasForeignKey(p => p.PtResultId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(e => e.Action)
                   .HasMaxLength(100)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumChangeCourseAction>());

            builder.Property(e => e.Status)
                   .HasMaxLength(100)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumChangingStatus>());
        }
    }
}

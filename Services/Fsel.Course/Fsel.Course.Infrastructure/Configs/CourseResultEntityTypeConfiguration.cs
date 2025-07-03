// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CourseResultEntityTypeConfiguration : IEntityTypeConfiguration<CourseResult>
    {
        public void Configure(EntityTypeBuilder<CourseResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumResultStatus>());

            builder.Property(e => e.WorkingStatus)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumWorkingStatus>());

            builder.HasOne(a => a.Course)
                 .WithMany(b => b.CourseResults)
                 .HasForeignKey(p => p.CourseId)
                 .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => new { c.CourseId, c.StudentId }).IsUnique().HasFilter("[IsDeleted] = 0");
            builder.HasIndex(c => new { c.StudentId, c.WorkingStatus });
            builder.HasIndex(c => new { c.IsDeleted, c.WorkingStatus });
            builder.HasIndex(c => new { c.CreatedUserId }).IncludeValueProperties(x => new { x.Status, x.CourseId });
        }
    }
}

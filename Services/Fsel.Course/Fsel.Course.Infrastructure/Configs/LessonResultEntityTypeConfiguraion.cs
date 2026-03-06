// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class LessonResultEntityTypeConfiguraion : IEntityTypeConfiguration<LessonResult>
    {
        public void Configure(EntityTypeBuilder<LessonResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Course)
                .WithMany(b => b.LessonResults)
                .HasForeignKey(b => b.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Unit)
                .WithMany(b => b.LessonResults)
                .HasForeignKey(b => b.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Lesson)
                .WithMany(b => b.LessonResults)
                .HasForeignKey(b => b.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.UnitModule)
                   .WithMany(b => b.LessonResults)
                   .HasForeignKey(b => b.UnitModuleId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.UnitResult)
                   .WithMany(b => b.LessonResults)
                   .HasForeignKey(b => b.UnitResultId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.Property(e => e.Status)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumResultStatus>());

            builder.HasIndex(c => new { c.CreatedUserId, c.Status, c.UnitId });
            builder.HasIndex(c => new { c.CourseId, c.StudentId, c.Status });
            builder.HasIndex(c => new { c.UnitModuleId, c.UnitResultId }).IsUnique().HasFilter("[IsDeleted] = 0");
        }
    }
}

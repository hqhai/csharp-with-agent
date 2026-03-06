// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UnitResultEntityTypeConfiguraion : IEntityTypeConfiguration<UnitResult>
    {
        public void Configure(EntityTypeBuilder<UnitResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            //builder.HasOne(a => a.Course)
            //    .WithMany(b => b.UnitResults)
            //    .HasForeignKey(b => b.CourseId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //builder.HasOne(a => a.Unit)
            //    .WithMany(b => b.UnitResults)
            //    .HasForeignKey(b => b.UnitId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //builder.Property(e => e.Status)
            //    .HasMaxLength(100)
            //    .HasConversion(
            //        v => v.ToString(),
            //        v => v.EnumParse<EnumResultStatus>());

            builder.HasOne(a => a.CourseModule)
                   .WithMany(b => b.UnitResults)
                   .HasForeignKey(b => b.CourseModuleId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.CourseResult)
                   .WithMany(b => b.UnitResults)
                   .HasForeignKey(b => b.CourseResultId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(c => new { c.StudentId, c.Status });

            builder.HasIndex(c => new { c.CourseResultId, c.CourseModuleId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0 AND [CourseResultId] IS NOT NULL AND [CourseModuleId] IS NOT NULL");
        }
    }
}

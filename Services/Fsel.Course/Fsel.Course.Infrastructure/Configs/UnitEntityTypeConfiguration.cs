// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Enums;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class UnitEntityTypeConfiguration : IEntityTypeConfiguration<Unit>
    {
        public void Configure(EntityTypeBuilder<Unit> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.CourseLevel)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseLevel>());

            builder.Property(e => e.VersionStatus)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumVersionStatus>());

            builder.HasOne(a => a.Program)
                .WithMany(b => b.Units)
                .HasForeignKey(a => a.ProgramId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Level)
                .WithMany(b => b.Units)
                .HasForeignKey(a => a.LevelId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

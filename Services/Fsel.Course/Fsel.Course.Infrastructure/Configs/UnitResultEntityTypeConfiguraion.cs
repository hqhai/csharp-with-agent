// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UnitResultEntityTypeConfiguraion : IEntityTypeConfiguration<UnitResult>
    {
        public void Configure(EntityTypeBuilder<UnitResult> builder)
        {
            builder.HasOne(a => a.Course)
                .WithMany(b => b.UnitResults)
                .HasForeignKey(b => b.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Unit)
                .WithMany(b => b.UnitResults)
                .HasForeignKey(b => b.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumResultStatus>());
        }
    }
}

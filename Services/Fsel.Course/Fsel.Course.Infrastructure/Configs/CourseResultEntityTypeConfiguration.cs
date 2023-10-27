// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CourseResultEntityTypeConfiguration : IEntityTypeConfiguration<CourseResult>
    {
        public void Configure(EntityTypeBuilder<CourseResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumResultStatus>());

            builder.HasOne(a => a.Course)
                 .WithMany(b => b.CourseResults)
                 .HasForeignKey(p => p.CourseId)
                 .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.CourseId).IsUnique(false);
        }
    }
}

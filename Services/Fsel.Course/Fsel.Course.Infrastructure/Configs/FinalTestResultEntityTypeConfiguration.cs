// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class FinalTestResultEntityTypeConfiguration : IEntityTypeConfiguration<FinalTestResult>
    {
        public void Configure(EntityTypeBuilder<FinalTestResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.FinalTest)
                   .WithMany(b => b.FinalTestResults)
                   .HasForeignKey(b => b.FinalTestId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Course)
                .WithMany(b => b.FinalTestResults)
                .HasForeignKey(b => b.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumResultStatus>());

            builder.HasIndex(c => new { c.CourseId, c.FinalTestId, c.StudentId }).IsUnique().HasFilter("[IsDeleted] = 0");
            builder.HasIndex(c => new { c.StudentId, c.CourseId });
        }
    }
}

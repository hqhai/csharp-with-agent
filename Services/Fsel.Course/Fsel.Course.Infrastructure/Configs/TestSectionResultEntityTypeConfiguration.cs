// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TestSectionResultEntityTypeConfiguration : IEntityTypeConfiguration<TestSectionResult>
    {
        public void Configure(EntityTypeBuilder<TestSectionResult> builder)
        {
            builder.HasOne(a => a.TestResult)
                   .WithMany(b => b.SectionResults)
                   .HasForeignKey(p => p.TestResultId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.ParentTestSectionResult)
                   .WithMany(b => b.SectionResults)
                   .HasForeignKey(p => p.ParentTestSectionResultId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.TestSection)
                   .WithMany(b => b.SectionResults)
                   .HasForeignKey(p => p.TestSectionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(e => e.Status)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumResultStatus>());
        }
    }
}

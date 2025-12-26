// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TestScoreEntityTypeConfiguration : IEntityTypeConfiguration<TestScore>
    {
        public void Configure(EntityTypeBuilder<TestScore> builder)
        {
            builder.HasOne(a => a.TestResult)
                   .WithMany(b => b.TestScores)
                   .HasForeignKey(p => p.TestResultId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.TestSectionResult)
                   .WithMany(b => b.TestScores)
                   .HasForeignKey(p => p.TestSectionResultId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.TestSection)
                   .WithMany(b => b.TestScores)
                   .HasForeignKey(p => p.TestSectionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(e => e.Criteria)
                .HasMaxLength(30)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumTestScoreCriteria>());
        }
    }
}

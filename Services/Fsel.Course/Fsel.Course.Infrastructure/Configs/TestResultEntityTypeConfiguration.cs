// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TestResultEntityTypeConfiguration : IEntityTypeConfiguration<TestResult>
    {
        public void Configure(EntityTypeBuilder<TestResult> builder)
        {
            builder.HasOne(a => a.Test)
                   .WithMany(b => b.TestResults)
                   .HasForeignKey(p => p.TestId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.TestGroupResult)
                   .WithMany(b => b.TestResults)
                   .HasForeignKey(p => p.TestGroupResultId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.StepFlow)
                   .WithMany(b => b.TestResults)
                   .HasForeignKey(p => p.StepFlowId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.ActionFlow)
                   .WithMany(b => b.TestResults)
                   .HasForeignKey(p => p.ActionFlowId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(e => e.Status)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumResultStatus>());

            builder.HasIndex(c => new { c.TestGroupResultId, c.TestId, c.IsDeleted });

            builder.Property(x => x.Percent)
                   .HasComputedColumnSql(@"CASE WHEN [CorrectTotal] > 0 THEN ROUND(([CorrectCount] * 100.0) / [CorrectTotal], 0) ELSE 0 END", stored: true)
                   .ValueGeneratedOnAddOrUpdate()
                   .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TestAnswerEntityTypeConfiguration : IEntityTypeConfiguration<TestAnswer>
    {
        public void Configure(EntityTypeBuilder<TestAnswer> builder)
        {
            builder.HasOne(a => a.TestSectionResult)
                   .WithMany(b => b.TestAnswers)
                   .HasForeignKey(p => p.TestSectionResultId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Question)
                   .WithMany(b => b.TestAnswers)
                   .HasForeignKey(p => p.QuestionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.TestSection)
                   .WithMany(b => b.TestAnswers)
                   .HasForeignKey(p => p.TestSectionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.TestResult)
                 .WithMany(b => b.TestAnswers)
                 .HasForeignKey(p => p.TestResultId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Property(e => e.Status)
                  .HasMaxLength(20)
                  .HasConversion(
                      v => v.ToString(),
                      v => v.EnumParse<EnumAnswerStatus>());

            builder.HasIndex(c => new { c.TestSectionResultId, c.TestSectionId, c.QuestionId })
                   .IsUnique().HasFilter("[IsDeleted] = 0");
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class MockTestAnswerEntityTypeConfiguration : IEntityTypeConfiguration<MockTestAnswer>
    {
        public void Configure(EntityTypeBuilder<MockTestAnswer> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.SectionQuestion)
                .WithMany(b => b.MockTestAnswers)
                .HasForeignKey(b => b.SectionQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Section)
                .WithMany(b => b.MockTestAnswers)
                .HasForeignKey(b => b.SectionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.SectionTimeCode)
             .WithMany(b => b.MockTestAnswers)
             .HasForeignKey(b => b.SectionTimeCodeId)
             .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.MockTestResult)
                .WithMany(b => b.MockTestAnswers)
                .HasForeignKey(b => b.MockTestResultId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.SectionGroupResult)
                  .WithMany(b => b.MockTestAnswers)
                  .HasForeignKey(b => b.SectionGroupResultId)
                  .OnDelete(DeleteBehavior.NoAction);

            builder.Property(e => e.Status)
                  .HasMaxLength(20)
                  .HasConversion(
                      v => v.ToString(),
                      v => v.EnumParse<EnumAnswerStatus>());

            builder.Property(e => e.AnswerStr).IsRequired(false);

            builder.HasIndex(c => new { c.MockTestResultId, c.SectionGroupResultId, c.SectionQuestionId }).IsUnique().HasFilter("SectionQuestionId IS NOT NULL AND [IsDeleted] = 0");
            builder.HasIndex(c => new { c.MockTestResultId, c.SectionGroupResultId, c.SectionTimeCodeId }).IsUnique().HasFilter("SectionTimeCodeId IS NOT NULL AND [IsDeleted] = 0");
            builder.HasIndex(c => new { c.MockTestResultId, c.SectionGroupResultId, c.SectionId }).IsUnique().HasFilter("SectionId IS NOT NULL AND [IsDeleted] = 0");
        }
    }
}

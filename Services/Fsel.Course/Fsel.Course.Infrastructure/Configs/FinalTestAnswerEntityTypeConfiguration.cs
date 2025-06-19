// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class FinalTestAnswerEntityTypeConfiguration : IEntityTypeConfiguration<FinalTestAnswer>
    {
        public void Configure(EntityTypeBuilder<FinalTestAnswer> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.FinalTestResult)
                .WithMany(b => b.FinalTestAnswers)
                .HasForeignKey(b => b.FinalTestResultId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.SectionQuestion)
                .WithMany(b => b.FinalTestAnswers)
                .HasForeignKey(b => b.SectionQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.SectionGroupResult)
                  .WithMany(b => b.FinalTestAnswers)
                  .HasForeignKey(b => b.SectionGroupResultId)
                  .OnDelete(DeleteBehavior.NoAction);

            builder.Property(e => e.Status)
                  .HasMaxLength(20)
                  .HasConversion(
                      v => v.ToString(),
                      v => v.EnumParse<EnumAnswerStatus>());

            builder.HasIndex(c => new { c.FinalTestResultId, c.SectionQuestionId, c.SectionGroupResultId }).IsUnique().HasFilter("[IsDeleted] = 0");
            ;
        }
    }
}

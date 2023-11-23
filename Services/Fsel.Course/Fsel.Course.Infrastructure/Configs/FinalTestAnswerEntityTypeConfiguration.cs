// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities;
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
        }
    }
}

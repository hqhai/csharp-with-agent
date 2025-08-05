// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Configs
{
    using System;
    using Fsel.ExamPractice.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ExamPracticeScoreEntityTypeConfiguration : IEntityTypeConfiguration<ExamPracticeScore>
    {
        public void Configure(EntityTypeBuilder<ExamPracticeScore> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.ExamPracticeResult)
                .WithMany(b => b.ExamPracticeScores)
                .HasForeignKey(b => b.ExamPracticeResultId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.ExamPracticeSection)
                .WithMany(b => b.ExamPracticeScores)
                .HasForeignKey(b => b.ExamPracticeSectionId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

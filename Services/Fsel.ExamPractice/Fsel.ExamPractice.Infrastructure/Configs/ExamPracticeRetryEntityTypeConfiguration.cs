// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Configs
{
    using System;
    using Fsel.ExamPractice.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ExamPracticeRetryEntityTypeConfiguration : IEntityTypeConfiguration<ExamPracticeRetry>
    {
        public void Configure(EntityTypeBuilder<ExamPracticeRetry> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.ExamPractice)
                .WithMany(b => b.ExamPracticeRetrys)
                .HasForeignKey(b => b.ExamPracticeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

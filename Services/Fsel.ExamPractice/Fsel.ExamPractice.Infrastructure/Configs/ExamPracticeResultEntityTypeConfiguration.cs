// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ExamPracticeResultEntityTypeConfiguration : IEntityTypeConfiguration<ExamPracticeResult>
    {
        public void Configure(EntityTypeBuilder<ExamPracticeResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.PracticeMode)
                 .HasMaxLength(20)
                 .HasConversion(
                     v => v.ToString(),
                     v => v.EnumParse<EnumPracticeMode>());

            builder.Property(e => e.Status)
                   .HasMaxLength(20)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumResultStatus>());

            builder.Property(e => e.WorkingStatus)
                 .HasMaxLength(20)
                 .HasConversion(
                     v => v.ToString(),
                     v => v.EnumParse<EnumWorkingStatus>());

            builder.HasOne(a => a.ExamPracticeRetry)
                .WithMany(b => b.ExamPracticeResults)
                .HasForeignKey(b => b.ExamPracticeRetryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.ExamPractice)
                .WithMany(b => b.ExamPracticeResults)
                .HasForeignKey(b => b.ExamPracticeId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

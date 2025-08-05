// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class QuestionEntityTypeConfiguration : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.QuestionType)
                 .HasMaxLength(50)
                 .HasConversion(
                     v => v.ToString(),
                     v => v.EnumParse<EnumQuestionType>());

            builder.HasOne(a => a.ExamPracticeSection)
                .WithMany(b => b.Questions)
                .HasForeignKey(b => b.ExamPracticeSectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

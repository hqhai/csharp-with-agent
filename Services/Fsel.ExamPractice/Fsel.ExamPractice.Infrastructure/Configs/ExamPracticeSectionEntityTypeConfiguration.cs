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

    public class ExamPracticeSectionEntityTypeConfiguration : IEntityTypeConfiguration<ExamPracticeSection>
    {
        public void Configure(EntityTypeBuilder<ExamPracticeSection> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.CourseSkill)
                 .HasMaxLength(20)
                 .HasConversion(
                     v => v.ToString(),
                     v => v.EnumParse<EnumCourseSkill>());

            builder.Property(e => e.Type)
                .HasMaxLength(50)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumSectionExamPracticeType>());

            builder.HasOne(a => a.ExamPractice)
                .WithMany(b => b.ExamPracticeSections)
                .HasForeignKey(b => b.ExamPracticeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.ParentExamPracticeSection)
                .WithMany(b => b.ExamPracticeSections)
                .HasForeignKey(b => b.ParentExamPracticeSectionId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

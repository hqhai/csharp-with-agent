// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ExamPracticeAiCriteriaSettingEntityTypeConfiguration : IEntityTypeConfiguration<ExamPracticeAICriteriaSetting>
    {
        public void Configure(EntityTypeBuilder<ExamPracticeAICriteriaSetting> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.CriteriaName)
              .HasMaxLength(50)
              .HasConversion(
                  v => v.ToString(),
                  v => v.EnumParse<EnumExamPracticeAIType>());

            builder.HasOne(a => a.ExamPracticeAISetting)
                   .WithMany(b => b.ExamPracticeAICriteriaSettings)
                   .HasForeignKey(b => b.ExamPracticeAISettingId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

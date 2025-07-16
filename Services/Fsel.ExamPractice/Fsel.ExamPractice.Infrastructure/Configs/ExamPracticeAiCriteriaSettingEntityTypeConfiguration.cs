// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Configs
{
    using Fsel.ExamPractice.Domain.Entities;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;

    public class ExamPracticeAiCriteriaSettingEntityTypeConfiguration : IEntityTypeConfiguration<ExamPracticeAICriteriaSetting>
    {
        public void Configure(EntityTypeBuilder<ExamPracticeAICriteriaSetting> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.CriteriaName)
              .HasMaxLength(50)
              .HasConversion(
                  v => v.ToString(),
                  v => v.EnumParse<EnumMockTestAIType>());

            builder.HasOne(a => a.ExamPracticeAISetting)
                .WithMany(b => b.ExamPracticeAICriteriaSettings)
                .HasForeignKey(b => b.ExamPracticeAISettingId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Configs
{
    using Fsel.ExamPractice.Domain.Entities;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.EntityFrameworkCore;

    public class ExamPracticeAISettingEntityTypeConfiguration : IEntityTypeConfiguration<ExamPracticeAISetting>
    {
        public void Configure(EntityTypeBuilder<ExamPracticeAISetting> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.ExamPracticeSection)
                .WithMany(b => b.ExamPracticeAISettings)
                .HasForeignKey(b => b.ExamPracticeSectionId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

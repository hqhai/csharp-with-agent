// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ProsodyScoreEntityTypeConfiguration : IEntityTypeConfiguration<ProsodyScore>
    {
        public void Configure(EntityTypeBuilder<ProsodyScore> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.AIType)
                  .HasMaxLength(50)
                  .HasConversion(
                      v => v.ToString(),
                      v => v.EnumParse<EnumExamPracticeAIType>());

            builder.Property(e => e.ModuleAIType)
                  .HasMaxLength(50)
                  .HasConversion(
                      v => v.ToString(),
                      v => v.EnumParse<EnumExamPracticeModuleAIType>());
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class SurveyQuestionEntityTypeConfiguration : IEntityTypeConfiguration<SurveyQuestion>
    {
        public void Configure(EntityTypeBuilder<SurveyQuestion> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Type)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumSurveyQuestion>());

            builder.Property(e => e.SurveyFormType)
                   .HasMaxLength(100)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumSurveyFormType>());

            builder.HasOne(a => a.SurveyConfig)
               .WithMany(b => b.SurveyQuestions)
               .HasForeignKey(b => b.SurveyConfigId)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

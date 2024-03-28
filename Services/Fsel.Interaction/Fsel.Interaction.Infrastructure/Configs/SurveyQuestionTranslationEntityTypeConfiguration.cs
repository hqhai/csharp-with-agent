// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Configs
{
    using Fsel.Interaction.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class SurveyQuestionTranslationEntityTypeConfiguration : IEntityTypeConfiguration<SurveyQuestionTranslation>
    {
        public void Configure(EntityTypeBuilder<SurveyQuestionTranslation> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.SurveyQuestion)
                  .WithMany(b => b.Translations)
                  .HasForeignKey(p => p.SurveyQuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

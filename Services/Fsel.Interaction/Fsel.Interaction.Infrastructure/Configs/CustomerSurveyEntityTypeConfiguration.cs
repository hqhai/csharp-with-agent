// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Configs
{
    using Fsel.Interaction.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CustomerSurveyEntityTypeConfiguration : IEntityTypeConfiguration<CustomerSurvey>
    {
        public void Configure(EntityTypeBuilder<CustomerSurvey> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.SurveyQuestion)
                  .WithMany(b => b.CustomerSurveys)
                  .HasForeignKey(b => b.SurveyQuestionId)
                  .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.CustomerSurveyGroup)
                   .WithMany(b => b.CustomerSurveys)
                   .HasForeignKey(b => b.CustomerSurveyGroupId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(c => new { c.IsDeleted, c.UserId });
        }
    }
}

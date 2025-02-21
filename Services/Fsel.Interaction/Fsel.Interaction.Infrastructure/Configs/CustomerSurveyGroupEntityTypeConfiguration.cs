// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CustomerSurveyGroupEntityTypeConfiguration : IEntityTypeConfiguration<CustomerSurveyGroup>
    {
        public void Configure(EntityTypeBuilder<CustomerSurveyGroup> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                   .HasMaxLength(100)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumSurveyGroupStatus>());

            builder.Property(e => e.SurveyFormType)
                   .HasMaxLength(100)
                   .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumSurveyFormType>());

            builder.HasIndex(c => new { c.IsDeleted, c.UserId, c.SurveyFormType });
        }
    }
}

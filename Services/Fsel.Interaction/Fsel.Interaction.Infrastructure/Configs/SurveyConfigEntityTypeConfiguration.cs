using Fsel.Common.Helpers;
using Fsel.Interaction.Domain.Entities;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Interaction.Infrastructure.Configs
{
    public class SurveyConfigEntityTypeConfiguration : IEntityTypeConfiguration<SurveyConfig>
    {
        public void Configure(EntityTypeBuilder<SurveyConfig> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.Status)
           .HasMaxLength(100)
           .HasConversion(
               v => v.ToString(),
               v => v.EnumParse<EnumSurveyConfigStatus>());
        }
    }
}

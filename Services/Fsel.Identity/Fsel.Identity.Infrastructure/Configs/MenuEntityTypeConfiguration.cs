using Fsel.Common.Helpers;
using Fsel.Identity.Domain.Entities;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Identity.Infrastructure.Configs
{
    public class MenuEntityTypeConfiguration : IEntityTypeConfiguration<Menu>
    {
        public void Configure(EntityTypeBuilder<Menu> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.Category)
                  .HasMaxLength(100)
                  .HasConversion(
                       v => v.ToString(),
                       v => v.EnumParse<EnumMenuCategory>());
        }
    }
}

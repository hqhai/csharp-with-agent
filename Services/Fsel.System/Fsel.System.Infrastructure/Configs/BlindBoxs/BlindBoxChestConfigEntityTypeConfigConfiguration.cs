using Fsel.Common.Helpers;
using Fsel.Shared.Enums;
using Fsel.System.Domain.Entities.BlindBoxs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.System.Infrastructure.Configs.BlindBoxs
{
    public class BlindBoxChestConfigEntityTypeConfigConfiguration : IEntityTypeConfiguration<BlindBoxChestConfig>
    {
        public void Configure(EntityTypeBuilder<BlindBoxChestConfig> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.ConfigType)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumBlindBoxConfigType>());

            builder.HasOne(a => a.BlindBoxChest)
                   .WithMany(b => b.BlindBoxChestConfigs)
                   .HasForeignKey(b => b.BlindBoxChestId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

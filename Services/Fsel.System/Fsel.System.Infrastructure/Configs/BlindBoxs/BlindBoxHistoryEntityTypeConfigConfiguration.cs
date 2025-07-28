using Fsel.System.Domain.Entities.BlindBoxs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.System.Infrastructure.Configs.BlindBoxs
{
    public class BlindBoxHistoryEntityTypeConfigConfiguration : IEntityTypeConfiguration<BlindBoxHistory>
    {
        public void Configure(EntityTypeBuilder<BlindBoxHistory> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.BlindBoxChestConfig)
                   .WithMany(b => b.BlindBoxHistories)
                   .HasForeignKey(b => b.BlindBoxChestConfigId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

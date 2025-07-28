using Fsel.System.Domain.Entities.BlindBoxs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.System.Infrastructure.Configs.BlindBoxs
{
    public class BlindBoxChestEntityTypeConfigConfiguration : IEntityTypeConfiguration<BlindBoxChest>
    {
        public void Configure(EntityTypeBuilder<BlindBoxChest> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.BlindBox)
                   .WithMany(b => b.BlindBoxChests)
                   .HasForeignKey(b => b.BlindBoxId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

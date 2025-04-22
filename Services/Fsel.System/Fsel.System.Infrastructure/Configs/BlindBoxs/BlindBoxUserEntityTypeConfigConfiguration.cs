using Fsel.System.Domain.Entities.BlindBoxs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.System.Infrastructure.Configs.BlindBoxs
{
    public class BlindBoxUserEntityTypeConfigConfiguration : IEntityTypeConfiguration<BlindBoxUser>
    {
        public void Configure(EntityTypeBuilder<BlindBoxUser> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.BlindBox)
                   .WithMany(b => b.BlindBoxUsers)
                   .HasForeignKey(b => b.BlindBoxId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

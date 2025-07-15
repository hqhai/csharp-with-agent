using Fsel.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Identity.Infrastructure.Configs
{
    public class RoleClaimEntityTypeConfiguration : IEntityTypeConfiguration<RoleClaim>
    {
        public void Configure(EntityTypeBuilder<RoleClaim> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.PermissionGroup)
                        .WithMany(b => b.RoleClaims)
                        .HasForeignKey(b => b.PermissionGroupId)
                        .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Permission)
                     .WithMany(b => b.RoleClaims)
                     .HasForeignKey(b => b.PermissionId)
                     .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

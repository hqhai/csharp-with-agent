using Fsel.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Identity.Infrastructure.Configs
{
    public class PermissionEntityTypeConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.PermissionGroup)
            .WithMany(b => b.Permissions)
            .HasForeignKey(b => b.PermissionGroupId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

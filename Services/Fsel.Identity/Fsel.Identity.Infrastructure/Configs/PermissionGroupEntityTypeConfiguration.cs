namespace Fsel.Identity.Infrastructure.Configs
{
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class PermissionGroupEntityTypeConfiguration : IEntityTypeConfiguration<PermissionGroup>
    {
        public void Configure(EntityTypeBuilder<PermissionGroup> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Menu)
                .WithOne(b => b.PermissionGroup)
                .HasForeignKey<PermissionGroup>(b => b.MenuId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

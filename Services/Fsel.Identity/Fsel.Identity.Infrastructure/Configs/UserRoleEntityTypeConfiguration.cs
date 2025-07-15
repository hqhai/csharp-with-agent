// Copyright (c) Atlantic. All rights reserved.

using Fsel.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Identity.Infrastructure.Configs
{
    public class UserRoleEntityTypeConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            // Thiết lập quan hệ với User
            builder.HasOne(ur => ur.User)
                .WithMany()
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Thiết lập quan hệ với Role
            builder.HasOne(ur => ur.Role)
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình bảng
            builder.ToTable("AspNetUserRoles");
        }
    }
}

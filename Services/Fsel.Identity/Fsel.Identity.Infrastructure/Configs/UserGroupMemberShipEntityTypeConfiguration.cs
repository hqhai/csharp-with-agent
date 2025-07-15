// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UserGroupMemberShipEntityTypeConfiguration : IEntityTypeConfiguration<UserGroupMemberShip>
    {
        public void Configure(EntityTypeBuilder<UserGroupMemberShip> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            // Thiết lập mối quan hệ với User
            builder.HasOne(a => a.User)
                   .WithMany(b => b.UserGroups)
                   .HasForeignKey(p => p.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Thiết lập mối quan hệ với UserGroup
            builder.HasOne(a => a.Group)
                   .WithMany(b => b.Members)
                   .HasForeignKey(p => p.GroupId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Các index để tối ưu hóa truy vấn
            builder.HasIndex(x => x.UserId).IsUnique(false);
            builder.HasIndex(x => x.GroupId).IsUnique(false);
            builder.HasIndex(x => x.IsActive).IsUnique(false);
            builder.HasIndex(x => new { x.UserId, x.GroupId, x.IsActive });
        }
    }
} 
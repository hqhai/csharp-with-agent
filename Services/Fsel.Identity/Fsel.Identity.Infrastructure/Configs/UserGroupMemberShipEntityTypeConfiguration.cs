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

        }
    }
} 

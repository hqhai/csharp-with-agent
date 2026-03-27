// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Helpers;
using Fsel.Master.Identity.Domain.Entities;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Fsel.Master.Identity.Infrastructure.Configs
{
    public class MasterUserEntityTypeConfiguration : IEntityTypeConfiguration<MasterUser>
    {
        public void Configure(EntityTypeBuilder<MasterUser> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.Gender)
                 .HasMaxLength(100)
                 .HasConversion(
                    v => v.HasValue ? v.ToString() : null,
                    v => v.EnumParse<EnumGender>());

            builder.Property(e => e.Status)
                 .HasMaxLength(100)
                 .HasConversion(
                    v => v.HasValue ? v.ToString() : null,
                    v => v.EnumParse<EnumUserStatus>());

            builder.Property(x => x.FullName)
                .HasComputedColumnSql($"CONCAT_WS(' ', [{nameof(MasterUser.LastName)}], [{nameof(MasterUser.FirstName)}])", stored: true);

            builder.HasIndex(x => new { x.IsDeleted, x.UserName });
            builder.HasIndex(x => new { x.IsDeleted, x.Email });
            builder.HasIndex(x => new { x.IsDeleted, x.PhoneNumber });
            builder.HasIndex(x => new { x.IsDeleted, x.Id, x.UserName, x.Email });
            builder.HasIndex(x => new { x.Id, x.ConcurrencyStamp });
        }
    }
}

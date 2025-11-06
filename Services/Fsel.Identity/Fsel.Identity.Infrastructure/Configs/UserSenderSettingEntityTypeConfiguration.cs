// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UserSenderSettingEntityTypeConfiguration : IEntityTypeConfiguration<UserSenderSetting>
    {
        public void Configure(EntityTypeBuilder<UserSenderSetting> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.UserSetting)
                   .WithMany(b => b.UserSenderSettings)
                   .HasForeignKey(p => p.UserSettingId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

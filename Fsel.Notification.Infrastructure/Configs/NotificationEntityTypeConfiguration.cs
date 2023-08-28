// Copyright (c) Atlantic. All rights reserved.

namespace FiveSIS.Notification.Infrastructure.Configs
{
    using FiveSIS.Notification.Domain.Entities;
    using FiveSIS.Shared.Enums;
    using Fsel.Common.Helpers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class NotificationEntityTypeConfiguration : IEntityTypeConfiguration<Notifications>
    {
        public void Configure(EntityTypeBuilder<Notifications> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumNotificationStatus>());

            builder.HasOne(a => a.NotificationType)
               .WithMany(b => b.Notifications)
               .HasForeignKey(b => b.NotificationTypeId)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Infrastructure.Configs
{
    using Fsel.Notification.Domain.Entities;
    using Fsel.Shared.Enums;
    using Fsel.Common.Helpers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class NotificationEntityTypeConfiguration : IEntityTypeConfiguration<NotificationMessage>
    {
        public void Configure(EntityTypeBuilder<NotificationMessage> builder)
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

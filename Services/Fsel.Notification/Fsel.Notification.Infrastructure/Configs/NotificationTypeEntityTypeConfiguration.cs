// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Infrastructure.Configs
{
    using Fsel.Notification.Domain.Entities;
    using Fsel.Shared.Enums;
    using Fsel.Common.Helpers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class NotificationTypeEntityTypeConfiguration : IEntityTypeConfiguration<NotificationType>
    {
        public void Configure(EntityTypeBuilder<NotificationType> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Type)
               .HasMaxLength(100)
               .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumNotificationPushingType>());
            builder.Property(e => e.Content)
               .HasMaxLength(100)
               .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumNotificationContent>());


        }
    }
}

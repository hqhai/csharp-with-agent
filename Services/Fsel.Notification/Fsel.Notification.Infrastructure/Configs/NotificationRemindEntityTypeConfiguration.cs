// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Infrastructure.Configs
{
    using Fsel.Notification.Domain.Entities;
    using Fsel.Shared.Enums;
    using Fsel.Common.Helpers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class NotificationRemindEntityTypeConfiguration : IEntityTypeConfiguration<NotificationRemind>
    {
        public void Configure(EntityTypeBuilder<NotificationRemind> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumNotificationRemindStatus>());
        }
    }
}

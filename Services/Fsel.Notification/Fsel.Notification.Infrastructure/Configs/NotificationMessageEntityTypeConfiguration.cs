// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Infrastructure.Configs
{
    using Fsel.Notification.Domain.Entities;
    using Fsel.Shared.Enums;
    using Fsel.Common.Helpers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class NotificationMessageEntityTypeConfiguration : IEntityTypeConfiguration<NotificationMessage>
    {
        public void Configure(EntityTypeBuilder<NotificationMessage> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumNotificationStatus>());

            builder.HasIndex(c => new { c.IsDeleted, c.UserId });
            builder.HasIndex(c => new { c.IsDeleted, c.UserId, c.Status });
            builder.HasIndex(c => new { c.IsDeleted, c.UserId, c.SenderId });
        }
    }
}

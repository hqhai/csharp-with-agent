// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Infrastructure.Configs
{
    using Fsel.Notification.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class NotificationTypeTranslationEntityTypeConfiguration : IEntityTypeConfiguration<NotificationTypeTranslation>
    {
        public void Configure(EntityTypeBuilder<NotificationTypeTranslation> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.NotificationType)
                  .WithMany(b => b.Translations)
                  .HasForeignKey(b => b.NotificationTypeId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

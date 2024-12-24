// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Infrastructure.Configs
{
    using Fsel.Notification.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class NotificationMessageTranslationEntityTypeConfiguration : IEntityTypeConfiguration<NotificationMessageTranslation>
    {
        public void Configure(EntityTypeBuilder<NotificationMessageTranslation> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.NotificationMessage)
                  .WithMany(b => b.Translations)
                  .HasForeignKey(b => b.NotificationMessageId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

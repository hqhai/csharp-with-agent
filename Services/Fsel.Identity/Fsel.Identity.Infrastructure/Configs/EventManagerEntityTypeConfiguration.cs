// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class EventManagerEntityTypeConfiguration : IEntityTypeConfiguration<EventManager>
    {
        public void Configure(EntityTypeBuilder<EventManager> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            // Cấu hình mối quan hệ với User
            builder.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Cấu hình mối quan hệ với CompetitionEvent
            builder.HasOne(e => e.CompetitionEvent)
                .WithMany()
                .HasForeignKey(e => e.CompetitionEventId)
                .OnDelete(DeleteBehavior.NoAction);

            // Tạo index cho hiệu suất truy vấn
            builder.HasIndex(e => new { e.UserId, e.CompetitionEventId }).IsUnique();
        }
    }
} 
// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Configs
{
    using System;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.Enums;
    using Fsel.Common.Helpers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class EventEntityTypeConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Type)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumEventType>());
        }
    }
}

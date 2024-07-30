// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CompetitionEventsEntityTypeConfiguration : IEntityTypeConfiguration<CompetitionEvents>
    {
        public void Configure(EntityTypeBuilder<CompetitionEvents> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
        }
    }
}

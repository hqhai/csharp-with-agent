// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentCompetitionEntityTypeConfiguration : IEntityTypeConfiguration<StudentCompetitionSnapShot>
    {
        public void Configure(EntityTypeBuilder<StudentCompetitionSnapShot> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
        }
    }
}

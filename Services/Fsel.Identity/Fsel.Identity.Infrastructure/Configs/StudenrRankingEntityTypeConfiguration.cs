// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Shared.Enums;
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

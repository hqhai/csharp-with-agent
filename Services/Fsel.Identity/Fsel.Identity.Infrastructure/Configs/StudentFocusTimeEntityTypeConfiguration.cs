// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentFocusTimeEntityTypeConfiguration : IEntityTypeConfiguration<StudentFocusTime>
    {
        public void Configure(EntityTypeBuilder<StudentFocusTime> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
        }
    }
}

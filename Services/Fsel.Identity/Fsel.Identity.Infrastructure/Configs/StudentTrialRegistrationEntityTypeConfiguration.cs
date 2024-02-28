// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentTrialRegistrationEntityTypeConfiguration : IEntityTypeConfiguration<StudentTrialRegistration>
    {
        public void Configure(EntityTypeBuilder<StudentTrialRegistration> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.Status)
                   .HasMaxLength(100)
                   .HasConversion(
                        v => v.ToString(),
                        v => v.EnumParse<EnumTrialRegistrationStatus>());
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.Training.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ClassEntityTypeConfiguration : IEntityTypeConfiguration<Class>
    {
        public void Configure(EntityTypeBuilder<Class> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.Status)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumClassStatus>());

            builder.Property(e => e.TeacherApprovalStatus)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.HasValue ? v.ToString() : null,
                    v => !string.IsNullOrEmpty(v) ? v.EnumParse<EnumTeacherApprovalStatus>() : null);
        }
    }
}

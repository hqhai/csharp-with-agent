// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Domain.Entities;
    using Fsel.Common.Helpers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Shared.Enums;

    public class StausStudentGoalHistoryTypeConfiguration : IEntityTypeConfiguration<StatusStudentGoalHistory>
    {

        public void Configure(EntityTypeBuilder<StatusStudentGoalHistory> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.StatusStudentGoal)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumStatusStudentCampus>());
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentGoalSummaryEntityTypeConfiguration : IEntityTypeConfiguration<StudentGoalSummary>
    {
        public void Configure(EntityTypeBuilder<StudentGoalSummary> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.ProgressStatus)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumProgressStatus>());

            builder.HasOne(a => a.StudentGoalAggregate)
                .WithMany(b => b.StudentGoalSummaries)
                .HasForeignKey(b => b.StudentGoalAggregateId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

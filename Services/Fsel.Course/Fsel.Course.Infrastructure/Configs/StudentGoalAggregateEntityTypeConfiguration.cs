// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentGoalAggregateEntityTypeConfiguration : IEntityTypeConfiguration<StudentGoalAggregate>
    {
        public void Configure(EntityTypeBuilder<StudentGoalAggregate> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.CombinedProgress)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCombinedProgress>());

            builder.Property(e => e.CurrentCombinedProgress)
                 .HasMaxLength(100)
                 .HasConversion(
                     v => v.ToString(),
                     v => v.EnumParse<EnumCombinedProgress>());

            builder.HasOne(a => a.Level)
                   .WithMany(b => b.StudentGoalAggregates)
                   .HasForeignKey(b => b.LevelId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Program)
                   .WithMany(b => b.StudentGoalAggregates)
                   .HasForeignKey(b => b.ProgramId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TestGroupResultEntityTypeConfiguration : IEntityTypeConfiguration<TestGroupResult>
    {
        public void Configure(EntityTypeBuilder<TestGroupResult> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Course)
                   .WithMany(b => b.TestGroupResults)
                   .HasForeignKey(p => p.CourseId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Unit)
                   .WithMany(b => b.TestGroupResults)
                   .HasForeignKey(p => p.UnitId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.CourseResult)
                   .WithMany(b => b.TestGroupResults)
                   .HasForeignKey(p => p.CourseResultId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.UnitResult)
                   .WithMany(b => b.TestGroupResults)
                   .HasForeignKey(p => p.UnitResultId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.CourseModule)
                   .WithMany(b => b.TestGroupResults)
                   .HasForeignKey(p => p.CourseModuleId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.UnitModule)
                   .WithMany(b => b.TestGroupResults)
                   .HasForeignKey(p => p.UnitModuleId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Level)
                   .WithMany(b => b.TestGroupResults)
                   .HasForeignKey(p => p.LevelId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.CurrentLevel)
                   .WithMany(b => b.TestGroupResultsForCurrentLevel)
                   .HasForeignKey(p => p.CurrentLevelId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.EmailLevel)
                   .WithMany(b => b.TestGroupResultsForEmailLevel)
                   .HasForeignKey(p => p.EmailLevelId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Flow)
                   .WithMany(b => b.TestGroupResults)
                   .HasForeignKey(p => p.FlowId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Category)
                   .WithMany(b => b.TestGroupResults)
                   .HasForeignKey(p => p.ProgramId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.Property(e => e.TestType)
                   .HasMaxLength(20)
                   .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumTestType>());

            builder.Property(e => e.Status)
                   .HasMaxLength(20)
                   .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumResultStatus>());

            builder.HasIndex(c => new { c.CourseModuleId, c.CourseResultId }).IsUnique().HasFilter("[IsDeleted] = 0 AND [UnitResultId] IS NULL");
            builder.HasIndex(c => new { c.UnitModuleId, c.UnitResultId }).IsUnique().HasFilter("[IsDeleted] = 0");
        }
    }
}

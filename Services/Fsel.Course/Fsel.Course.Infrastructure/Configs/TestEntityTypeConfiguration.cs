// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Amazon.S3;
    using Fsel.Common.Enums;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TestEntityTypeConfiguration : IEntityTypeConfiguration<Test>
    {
        public void Configure(EntityTypeBuilder<Test> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Program)
                   .WithMany(b => b.Tests)
                   .HasForeignKey(p => p.ProgramId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Level)
                   .WithMany(b => b.Tests)
                   .HasForeignKey(p => p.LevelId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.Property(e => e.VersionStatus)
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumVersionStatus>());
        }
    }
}

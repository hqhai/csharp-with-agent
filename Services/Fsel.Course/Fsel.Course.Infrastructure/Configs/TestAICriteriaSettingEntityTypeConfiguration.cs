// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TestAICriteriaSettingEntityTypeConfiguration : IEntityTypeConfiguration<TestAICriteriaSetting>
    {
        public void Configure(EntityTypeBuilder<TestAICriteriaSetting> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.TestAISetting)
                .WithMany(b => b.TestAICriteriaSettings)
                .HasForeignKey(b => b.TestAISettingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.CriteriaName)
                .HasMaxLength(30)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumMockTestAIType>());
        }
    }
}

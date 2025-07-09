// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TestAISettingEntityTypeConfiguration : IEntityTypeConfiguration<TestAISetting>
    {
        public void Configure(EntityTypeBuilder<TestAISetting> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.TestSection)
                   .WithMany(b => b.TestAISettings)
                   .HasForeignKey(p => p.TestSectionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

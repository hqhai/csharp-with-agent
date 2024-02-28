// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class MockTestAISettingTypeConfiguration : IEntityTypeConfiguration<MockTestAISetting>
    {
        public void Configure(EntityTypeBuilder<MockTestAISetting> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.Section)
                   .WithOne(b=>b.MockTestAISetting)
                   .HasForeignKey<MockTestAISetting>(b => b.SectionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

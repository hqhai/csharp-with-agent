// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Fsel.Shared.Enums;

    public class MockTestAISettingTypeConfiguration : IEntityTypeConfiguration<MockTestAISetting>
    {
        public void Configure(EntityTypeBuilder<MockTestAISetting> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.Criteria)
                  .HasMaxLength(100)
                  .HasConversion(
                      v => v.ToString(),
                      v => v.EnumParse<EnumMockTestAIType>());

            builder.HasOne(a => a.Section)
                   .WithMany(b => b.MockTestAISettings)
                   .HasForeignKey(b => b.SectionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

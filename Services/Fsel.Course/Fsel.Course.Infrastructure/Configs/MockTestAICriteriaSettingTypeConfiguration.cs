// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Fsel.Shared.Enums;

    public class MockTestAICriteriaSettingTypeConfiguration : IEntityTypeConfiguration<MockTestAICriteriaSetting>
    {
        public void Configure(EntityTypeBuilder<MockTestAICriteriaSetting> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.CriteriaName)
                   .HasMaxLength(100)
                    .HasConversion(
                        v => v.ToString(),
                        v => v.EnumParse<EnumMockTestAIType>());

            builder.HasOne(a => a.MockTestAISetting)
                   .WithMany(b => b.MockTestAICriteriaSettings)
                   .HasForeignKey(b => b.MockTestAISettingId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

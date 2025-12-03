// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Domain.Entities.LongAnswerConfig;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class LongAnswerSettingTypeConfiguration : IEntityTypeConfiguration<LongAnswerSetting>
    {
        public void Configure(EntityTypeBuilder<LongAnswerSetting> builder)
        {

        }
    }
}

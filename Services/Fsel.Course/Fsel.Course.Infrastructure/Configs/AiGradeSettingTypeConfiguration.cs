// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AiGradeSettingTypeConfiguration : IEntityTypeConfiguration<AiGradeSetting>
    {
        public void Configure(EntityTypeBuilder<AiGradeSetting> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
        }
    }
}

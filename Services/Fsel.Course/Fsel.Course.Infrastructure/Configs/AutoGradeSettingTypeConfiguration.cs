// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AutoGradeSettingTypeConfiguration : IEntityTypeConfiguration<AutoGradeSetting>
    {
        public void Configure(EntityTypeBuilder<AutoGradeSetting> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
        }
    }
}

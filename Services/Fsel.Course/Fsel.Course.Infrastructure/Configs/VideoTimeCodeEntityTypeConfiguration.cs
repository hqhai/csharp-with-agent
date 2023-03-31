// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class VideoTimeCodeEntityTypeConfiguration : IEntityTypeConfiguration<VideoTimeCode>
    {
        public void Configure(EntityTypeBuilder<VideoTimeCode> builder)
        {
            builder.Property(e => e.TimeCodeType)
                  .HasMaxLength(100)
                  .HasConversion(
                      v => v.ToString(),
                      v => v.EnumParse<EnumTimeCodeType>());
        }
    }
}

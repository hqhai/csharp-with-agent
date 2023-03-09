using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Infrastructure.Configs
{
    public class VideoTimeCodeTypeConfiguration : IEntityTypeConfiguration<VideoTimeCode>
    {
        public void Configure(EntityTypeBuilder<VideoTimeCode> builder)
        {
            builder.Property(e => e.TimeCodeType)
                  .HasMaxLength(100)
                  .HasConversion(
                      v => v.ToString(),
                      v => (EnumTimeCodeType)Enum.Parse(typeof(EnumTimeCodeType), v));
        }
    }
}
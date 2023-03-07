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
    public class UnitEntityTypeConfiuration : IEntityTypeConfiguration<Unit>
    {
        public void Configure(EntityTypeBuilder<Unit> builder)
        {
            builder.Property(e => e.Type)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => (EnumUnitType)Enum.Parse(typeof(EnumUnitType), v));
        }
    }
}

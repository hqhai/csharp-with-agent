using Fsel.Common.Enums;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class UnitEntityTypeConfiguration : IEntityTypeConfiguration<Unit>
    {
        public void Configure(EntityTypeBuilder<Unit> builder)
        {
            builder.Property(e => e.CourseLevel)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => v.EnumParse<EnumCourseLevel>());
        }
    }
}

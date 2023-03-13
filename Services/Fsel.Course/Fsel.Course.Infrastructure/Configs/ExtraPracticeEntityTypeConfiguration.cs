using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class ExtraPracticeEntityTypeConfiguration : IEntityTypeConfiguration<ExtraPractice>
    {
        public void Configure(EntityTypeBuilder<ExtraPractice> builder)
        {
            builder.Property(e => e.Type)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => (EnumExtraPracticeType)Enum.Parse(typeof(EnumExtraPracticeType), v));
            builder.Property(e => e.CourseLevel)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => (EnumCourseLevel)Enum.Parse(typeof(EnumCourseLevel), v));
        }
    }
}
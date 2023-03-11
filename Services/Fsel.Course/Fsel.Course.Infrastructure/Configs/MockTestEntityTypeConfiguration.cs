using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class MockTestEntityTypeConfiguration : IEntityTypeConfiguration<MockTest>
    {
        public void Configure(EntityTypeBuilder<MockTest> builder)
        {
            builder.Property(e => e.CourseType)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => (EnumCourseType)Enum.Parse(typeof(EnumCourseType), v));

            builder.Property(e => e.MockTestType)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => (EnumMockTestType)Enum.Parse(typeof(EnumMockTestType), v));
        }
    }
}
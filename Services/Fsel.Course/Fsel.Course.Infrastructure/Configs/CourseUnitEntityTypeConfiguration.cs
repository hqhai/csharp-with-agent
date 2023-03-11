using Fsel.Course.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class CourseUnitEntityTypeConfiguration : IEntityTypeConfiguration<CourseUnit>
    {
        public void Configure(EntityTypeBuilder<CourseUnit> builder)
        {
            builder.HasOne(a => a.Unit)
                .WithMany(b => b.CourseUnits)
                .HasForeignKey(b => b.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Course)
                .WithMany(b => b.CourseUnits)
                .HasForeignKey(b => b.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
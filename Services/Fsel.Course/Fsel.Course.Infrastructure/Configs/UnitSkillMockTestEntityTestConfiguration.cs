using Fsel.Course.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class UnitSkillMockTestEntityTestConfiguration : IEntityTypeConfiguration<UnitSkillMockTest>
    {
        public void Configure(EntityTypeBuilder<UnitSkillMockTest> builder)
        {
            builder.HasOne(a => a.Unit)
                .WithMany(b => b.UnitSkillMockTests)
                .HasForeignKey(b => b.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            /*builder.HasOne(a => a.MockTest)
                .WithMany(b => b.CourseUnitMockTests)
                .HasForeignKey(b => b.MockTestId)
                .OnDelete(DeleteBehavior.Cascade);*/
        }
    }
}
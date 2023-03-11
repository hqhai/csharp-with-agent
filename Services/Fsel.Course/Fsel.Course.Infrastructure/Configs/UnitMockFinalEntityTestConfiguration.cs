using Fsel.Course.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class UnitMockFinalEntityTestConfiguration : IEntityTypeConfiguration<MockFinalTest>
    {
        public void Configure(EntityTypeBuilder<MockFinalTest> builder)
        {
            builder.HasOne(a => a.Unit)
                .WithMany(b => b.UnitMockFinalTests)
                .HasForeignKey(b => b.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            /*builder.HasOne(a => a.MockTest)
                .WithMany(b => b.CourseUnitMockTests)
                .HasForeignKey(b => b.MockTestId)
                .OnDelete(DeleteBehavior.Cascade);*/
        }
    }
}
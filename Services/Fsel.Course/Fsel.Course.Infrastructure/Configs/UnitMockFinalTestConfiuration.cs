using Fsel.Course.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class UnitMockFinalTestConfiuration : IEntityTypeConfiguration<UnitMockFinalTest>
    {
        public void Configure(EntityTypeBuilder<UnitMockFinalTest> builder)
        {
            builder.HasOne(a => a.Unit)
                .WithMany(b => b.UnitMockFinalTests)
                .HasForeignKey(b => b.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.MockFinalTest)
                .WithMany(b => b.UnitMockFinalTests)
                .HasForeignKey(b => b.MockFinalTestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
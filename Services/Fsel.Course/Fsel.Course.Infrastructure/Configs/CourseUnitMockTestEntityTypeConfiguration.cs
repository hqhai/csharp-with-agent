using Fsel.Course.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Infrastructure.Configs
{
    public class CourseUnitMockTestEntityTypeConfiguration : IEntityTypeConfiguration<CourseUnitMockTest>
    {
        public void Configure(EntityTypeBuilder<CourseUnitMockTest> builder)
        {
            builder.HasOne(a => a.Unit)
                .WithMany(b => b.CourseUnits)
                .HasForeignKey(b => b.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Course)
                .WithMany(b => b.CourseUnitMockTests)
                .HasForeignKey(b => b.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.MockTest)
                .WithMany(b => b.CourseUnitMockTests)
                .HasForeignKey(b => b.MockTestId)
                .OnDelete(DeleteBehavior.Cascade);

            /*builder.HasOne(a => a.MockTest)
                .WithMany(b => b.CourseUnitMockTests)
                .HasForeignKey(b => b.MockTestId)
                .OnDelete(DeleteBehavior.Cascade);*/
        }
    }
}
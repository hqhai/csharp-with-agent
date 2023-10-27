// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class CourseUnitMockTestEntityTypeConfiguration : IEntityTypeConfiguration<CourseUnitMockTest>
    {
        public void Configure(EntityTypeBuilder<CourseUnitMockTest> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.Unit)
                .WithMany(b => b.CourseUnitMockTests)
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

            builder.HasOne(a => a.FinalTest)
                .WithMany(b => b.CourseUnitMockTests)
                .HasForeignKey(b => b.FinalTestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

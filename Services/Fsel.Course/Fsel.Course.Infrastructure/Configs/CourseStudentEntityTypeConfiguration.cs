// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.EntityFrameworkCore;

    public class CourseStudentEntityTypeConfiguration : IEntityTypeConfiguration<CourseStudent>
    {
        public void Configure(EntityTypeBuilder<CourseStudent> builder)
        {
            builder.HasOne(a => a.Course)
                .WithMany(b => b.CourseStudentTrainings)
                .HasForeignKey(b => b.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

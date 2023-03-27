// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CourseClassEntityTypeConfiguration : IEntityTypeConfiguration<CourseClassStudent>
    {
        public void Configure(EntityTypeBuilder<CourseClassStudent> builder)
        {
            builder.HasOne(a => a.Course)
                .WithMany(b => b.CourseClasses)
                .HasForeignKey(b => b.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
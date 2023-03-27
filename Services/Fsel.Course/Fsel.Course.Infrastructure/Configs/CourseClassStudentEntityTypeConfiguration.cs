// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Configs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CourseClassStudentEntityTypeConfiguration : IEntityTypeConfiguration<CourseClassStudent>
    {
        public void Configure(EntityTypeBuilder<CourseClassStudent> builder)
        {
            builder.HasOne(a => a.Course)
                .WithMany(b => b.CourseClassStudents)
                .HasForeignKey(b => b.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

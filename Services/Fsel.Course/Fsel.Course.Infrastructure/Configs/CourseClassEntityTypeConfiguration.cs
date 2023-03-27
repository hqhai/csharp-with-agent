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

    public class CourseClassEntityTypeConfiguration : IEntityTypeConfiguration<CourseClass>
    {
        public void Configure(EntityTypeBuilder<CourseClass> builder)
        {
            builder.HasOne(a => a.Course)
                .WithMany(b => b.CourseClasses)
                .HasForeignKey(b => b.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

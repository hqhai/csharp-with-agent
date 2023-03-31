// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class LessonHomeWorkEntityTypeConfiguration : IEntityTypeConfiguration<LessonHomeWork>
    {
        public void Configure(EntityTypeBuilder<LessonHomeWork> builder)
        {
            builder.HasOne(a => a.Lesson)
                            .WithMany(b => b.LessonHomeWorks)
                            .HasForeignKey(b => b.LessonId)
                            .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.HomeWork)
                .WithMany(b => b.LessonHomeWorks)
                .HasForeignKey(b => b.HomeWorkId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

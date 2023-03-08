using Fsel.Course.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Infrastructure.Configs
{
    public class UnitLessonEntityTypeConfiguration : IEntityTypeConfiguration<UnitLesson>
    {
        public void Configure(EntityTypeBuilder<UnitLesson> builder)
        {
            builder.HasOne(a => a.Lesson)
                .WithMany(b => b.UnitLessons)
                .HasForeignKey(b => b.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Unit)
                .WithMany(b => b.UnitLessons)
                .HasForeignKey(b => b.UnitId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
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
    public class LessonUnitEntityTypeConfiguration : IEntityTypeConfiguration<LessonUnit>
    {
        public void Configure(EntityTypeBuilder<LessonUnit> builder)
        {
            builder.HasOne(a => a.Lesson)
                .WithMany(b => b.lessonUnits)
                .HasForeignKey(b => b.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Unit)
                .WithMany(b => b.lessonUnits)
                .HasForeignKey(b => b.UnitId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

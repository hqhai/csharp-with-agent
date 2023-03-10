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
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
    public class LessonExtraPracticeEntityTypeConfiguration : IEntityTypeConfiguration<LessonExtraPractice>
    {
        public void Configure(EntityTypeBuilder<LessonExtraPractice> builder)
        {
            builder.HasOne(a => a.Lesson)
                            .WithMany(b => b.LessonExtraPractices)
                            .HasForeignKey(b => b.LessonId)
                            .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.ExtraPractice)
                .WithMany(b => b.LessonExtraPractices)
                .HasForeignKey(b => b.ExtracPraticeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
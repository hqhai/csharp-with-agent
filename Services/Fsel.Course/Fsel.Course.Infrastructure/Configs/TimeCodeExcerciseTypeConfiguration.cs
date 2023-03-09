using Fsel.Course.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Infrastructure.Configs
{
    public class TimeCodeExcerciseTypeConfiguration : IEntityTypeConfiguration<TimeCodeExcercise>
    {
        public void Configure(EntityTypeBuilder<TimeCodeExcercise> builder)
        {
            builder.HasOne(a => a.VideoTimeCode)
                .WithMany(b => b.TimeCodeExcercises)
                .HasForeignKey(b => b.VideoTimeCodeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Excercise)
                .WithMany(b => b.TimeCodeExcercises)
                .HasForeignKey(b => b.ExcerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
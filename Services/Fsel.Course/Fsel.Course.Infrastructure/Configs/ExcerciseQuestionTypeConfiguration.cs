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
    public class ExcerciseQuestionTypeConfiguration : IEntityTypeConfiguration<ExcerciseQuestion>
    {
        public void Configure(EntityTypeBuilder<ExcerciseQuestion> builder)
        {
            builder.HasOne(a => a.Excercise)
                .WithMany(b => b.ExcerciseQuestions)
                .HasForeignKey(b => b.ExcerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Question)
                .WithMany(b => b.ExcerciseQuestions)
                .HasForeignKey(b => b.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
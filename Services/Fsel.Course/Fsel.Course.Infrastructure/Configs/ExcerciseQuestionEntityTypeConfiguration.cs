using Fsel.Course.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class ExcerciseQuestionEntityTypeConfiguration : IEntityTypeConfiguration<ExcerciseQuestion>
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
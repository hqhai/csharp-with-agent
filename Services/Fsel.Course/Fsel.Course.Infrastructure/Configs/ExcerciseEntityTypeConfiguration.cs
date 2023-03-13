using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class ExcerciseEntityTypeConfiguration : IEntityTypeConfiguration<Excercise>
    {
        public void Configure(EntityTypeBuilder<Excercise> builder)
        {
            builder.Property(e => e.QuestionType)
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => (EnumQuestionType)Enum.Parse(typeof(EnumQuestionType), v));

            builder.Property(e => e.CourseSkill)
               .HasMaxLength(100)
               .HasConversion(
                   v => v.ToString(),
                   v => (EnumCourseSkill)Enum.Parse(typeof(EnumCourseSkill), v));
        }
    }
}
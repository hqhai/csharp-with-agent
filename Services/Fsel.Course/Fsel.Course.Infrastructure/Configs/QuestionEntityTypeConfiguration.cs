using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class QuestionEntityTypeConfiguration : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.Property(e => e.QuestionType)
                 .HasMaxLength(100)
                 .HasConversion(
                     v => v.ToString(),
                     v => (EnumQuestionType)Enum.Parse(typeof(EnumQuestionType), v));
        }
    }
}
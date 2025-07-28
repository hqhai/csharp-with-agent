using Fsel.System.Domain.Entities.DailyQuiz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.System.Infrastructure.Configs
{
    public class DailyQuizAnswerEntityTypeConfigConfiguration : IEntityTypeConfiguration<DailyQuizAnswer>
    {
        public void Configure(EntityTypeBuilder<DailyQuizAnswer> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.DailyQuizQuestion)
                 .WithMany(b => b.DailyQuizAnswers)
                 .HasForeignKey(b => b.DailyQuizQuestionId)
                 .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

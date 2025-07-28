using Fsel.System.Domain.Entities.DailyQuiz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.System.Infrastructure.Configs
{
    public class DailyQuizHistoryEntityTypeConfigConfiguration : IEntityTypeConfiguration<DailyQuizHistory>
    {
        public void Configure(EntityTypeBuilder<DailyQuizHistory> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasOne(a => a.DailyQuizQuestion)
            .WithMany(b => b.DailyQuizHistories)
            .HasForeignKey(b => b.DailyQuizQuestionId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.DailyQuizAnswer)
           .WithMany(b => b.DailyQuizHistories)
           .HasForeignKey(b => b.DailyQuizAnswerId)
           .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

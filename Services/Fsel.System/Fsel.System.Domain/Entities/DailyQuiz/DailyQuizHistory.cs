using Fsel.Common.Helpers;
using Fsel.Core.Entities;

namespace Fsel.System.Domain.Entities.DailyQuiz
{
    public class DailyQuizHistory : Entity
    {
        public Guid DailyQuizQuestionId { get; set; }
        public Guid DailyQuizAnswerId { get; set; }
        public int? Index { get; set; }
        public DailyQuizQuestion? DailyQuizQuestion { get; set; }
        public DailyQuizAnswer? DailyQuizAnswer { get; set; }
        public DateTime? CreatedDateLocal { get; set; } = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date;
    }
}

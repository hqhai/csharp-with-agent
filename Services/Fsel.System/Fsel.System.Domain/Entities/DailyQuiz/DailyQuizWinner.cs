using Fsel.Common.Enums.ErrorCodes;
using System.ComponentModel.DataAnnotations;
using Fsel.Core.Entities;
using Fsel.Common.Helpers;

namespace Fsel.System.Domain.Entities.DailyQuiz
{
    public class DailyQuizWinner : Entity
    {
        [Required]
        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        public Guid SchoolId { get; set; }
        public Guid CompetitionEventId { get; set; }
        public bool IsWin { get; set; }
        public DateTime? CreatedDateLocal { get; set; } = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date;
    }
}

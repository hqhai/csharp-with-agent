using Fsel.Common.Helpers;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Core.Base.Interfaces;
using Fsel.Core.Entities;
using Fsel.Common.Enums.ErrorCodes;
using System.ComponentModel.DataAnnotations;

namespace Fsel.System.Domain.Entities.DailyQuiz
{
    public class DailyQuizQuestion : Entity, IMultiLingualObject<DailyQuizQuestionTranslation>
    {
        [Required]
        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Category { get; set; }

        [Required]
        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SubCategory { get; set; }

        [Required]
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Content { get; set; }

        [Required]
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Explanation { get; set; }

        public ICollection<DailyQuizAnswer> DailyQuizAnswers { get; set; } = new List<DailyQuizAnswer>();
        public ICollection<DailyQuizHistory> DailyQuizHistories { get; set; } = new List<DailyQuizHistory>();

        /// <summary>
        /// Đa ngôn ngữ
        /// </summary>
        [Required]
        [MaxLength(2000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? TranslationsStr { get; set; }

        [NotMapped]
        public ICollection<DailyQuizQuestionTranslation>? Translations
        {
            get { return TranslationsStr.Deserialize<ICollection<DailyQuizQuestionTranslation>>(); }
            set { TranslationsStr = value.Serialize(); }
        }
    }

    public class DailyQuizQuestionTranslation : ITranslationObject
    {
        public string? Content { get; set; }
        public string? Explanation { get; set; }
        public string? Language { get; set; }
    }
}

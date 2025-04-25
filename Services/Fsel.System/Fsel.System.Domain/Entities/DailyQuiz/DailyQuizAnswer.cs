namespace Fsel.System.Domain.Entities.DailyQuiz
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Core.Entities;
    using global::System.ComponentModel.DataAnnotations;
    using global::System.ComponentModel.DataAnnotations.Schema;

    public class DailyQuizAnswer : Entity, IMultiLingualObject<DailyQuizAnswerTranslation>
    {
        [Required]
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Content { get; set; }

        public bool IsCorrect { get; set; }
        public Guid DailyQuizQuestionId { get; set; }

        /// <summary>
        /// Đa ngôn ngữ
        /// </summary>
        [Required]
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? TranslationsStr { get; set; }

        [NotMapped]
        public ICollection<DailyQuizAnswerTranslation>? Translations
        {
            get { return TranslationsStr.Deserialize<ICollection<DailyQuizAnswerTranslation>>(); }
            set { TranslationsStr = value.Serialize(); }
        }

        public DailyQuizQuestion? DailyQuizQuestion { get; set; }
        public ICollection<DailyQuizHistory> DailyQuizHistories { get; set; } = new List<DailyQuizHistory>();
    }

    public class DailyQuizAnswerTranslation : ITranslationObject
    {
        public string? Content { get; set; }
        public string? Language { get; set; }
    }
}

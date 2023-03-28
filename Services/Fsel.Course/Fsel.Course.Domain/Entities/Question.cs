using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAnnotationsExtensions;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Common.Helpers;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Entities
{
    public class Question : Entity
    {
        /// <summary>
        /// Loại câu hỏi
        /// </summary>
        public EnumQuestionType QuestionType { get; set; }

        /// <summary>
        /// Tổng số câu trả lời đúng
        /// </summary>
        [Min(0, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int? CorrectTotal { get; set; }

        public bool IsSave { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? ConfigStr { get; set; }

        [NotMapped]
        public object? Config
        {
            get { return ConvertHelper.Deserialize<object>(ConfigStr); }
            set { ConfigStr = ConvertHelper.Serialize(value); }
        }

        public ICollection<ExerciseQuestion> ExerciseQuestions { get; set; } = new List<ExerciseQuestion>();
        public VideoTimeCodeAnswer? VideoTimeCodeAnswer { get; set; }
    }
}

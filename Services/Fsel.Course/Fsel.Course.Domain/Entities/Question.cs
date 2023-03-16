using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Common.Helpers;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;

namespace Fsel.Course.Domain.Entities
{
    public class Question : Entity
    {
        /// <summary>
        /// Loại câu hỏi
        /// </summary>
        public EnumQuestionType QuestionType { get; set; }

        public bool IsSave { get; set; }

        [Required(ErrorMessage = nameof(EnumQuestionErrorCode.Q01C))]
        [MaxLength(1000, ErrorMessage = nameof(EnumQuestionErrorCode.Q03C))]
        public string? ConfigStr { get; set; }

        [NotMapped]
        public object? Config
        {
            get { return ConvertHelper.Deserialize<object>(ConfigStr); }
            set { ConfigStr = ConvertHelper.Serialize(value); }
        }

        public ICollection<ExcerciseQuestion> ExcerciseQuestions { get; set; } = new List<ExcerciseQuestion>();
    }
}

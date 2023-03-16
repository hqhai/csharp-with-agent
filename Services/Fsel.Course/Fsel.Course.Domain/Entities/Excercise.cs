using System.ComponentModel.DataAnnotations;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;

namespace Fsel.Course.Domain.Entities
{
    public class Excercise : Entity
    {
        /// <summary>
        /// Tên Excercise
        /// </summary>
        [Required(ErrorMessage = nameof(EnumExcerciseErrorCode.E01C))]
        [MaxLength(250, ErrorMessage = nameof(EnumExcerciseErrorCode.E02C))]
        public string? Name { get; set; }

        /// <summary>
        /// Loại bài tập
        /// </summary>
        public EnumQuestionType QuestionType { get; set; }

        /// <summary>
        /// Media Post
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumExcerciseErrorCode.E03C))]
        public string? MediaPost { get; set; }

        /// <summary>
        /// Loại kỹ năng
        /// </summary>
        public EnumCourseSkill CourseSkill { get; set; }

        public ICollection<TimeCodeExcercise> TimeCodeExcercises { get; set; } = new List<TimeCodeExcercise>();
        public ICollection<ExcerciseQuestion> ExcerciseQuestions { get; set; } = new List<ExcerciseQuestion>();
    }
}

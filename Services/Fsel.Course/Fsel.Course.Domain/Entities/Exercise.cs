using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;

namespace Fsel.Course.Domain.Entities
{
    public class Exercise : Entity
    {
        /// <summary>
        /// Tên Exercise
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Media Post
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? MediaPost { get; set; }

        /// <summary>
        /// Loại kỹ năng
        /// </summary>
        public EnumCourseSkill CourseSkill { get; set; }

        public IList<TimeCodeExercise> TimeCodeExercises { get; set; } = new List<TimeCodeExercise>();
        public IList<ExerciseQuestion> ExerciseQuestions { get; set; } = new List<ExerciseQuestion>();
    }
}

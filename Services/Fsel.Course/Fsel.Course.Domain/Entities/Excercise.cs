using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Entities
{
    public class Excercise : Entity
    {
        [Required(ErrorMessage = nameof(EnumExcerciseErrorCode.E01V))]
        [MaxLength(250, ErrorMessage = nameof(EnumExcerciseErrorCode.E01C))]
        public string? Name { get; set; }

        public EnumQuestionType QuestionType { get; set; }

        [Required(ErrorMessage = nameof(EnumExcerciseErrorCode.E01V))]
        [MaxLength(1000, ErrorMessage = nameof(EnumExcerciseErrorCode.E03C))]
        public string? MediaPost { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }

        public List<TimeCodeExcercise> TimeCodeExcercises { get; set; } = new List<TimeCodeExcercise>();
        public List<ExcerciseQuestion> ExcerciseQuestions { get; set; } = new List<ExcerciseQuestion>();
    }
}
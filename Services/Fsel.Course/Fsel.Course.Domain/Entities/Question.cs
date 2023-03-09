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
    public class Question : Entity
    {
        public EnumQuestionType QuestionType { get; set; }
        public bool IsSave { get; set; }

        [Required(ErrorMessage = nameof(EnumQuestionErrorCode.Q01V))]
        [MaxLength(1000, ErrorMessage = nameof(EnumQuestionErrorCode.Q03C))]
        public string? Config { get; set; }

        public List<ExcerciseQuestion> ExcerciseQuestions { get; set; } = new List<ExcerciseQuestion>();
    }
}
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Common.Models.Commands.Excercise
{
    public class CreateExcerciseCommandModel
    {
        public string? Name { get; set; }

        public EnumQuestionType QuestionType { get; set; }

        public string? MediaPost { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }

        public List<ExcerciseQuestion> ExcerciseQuestions { get; set; } = new List<ExcerciseQuestion>();
    }
}
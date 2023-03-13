using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Models.CommandModels.Questions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Models.CommandModels.Excercises
{
    public class CreateExcerciseCommandModel
    {
        public string? Name { get; set; }

        public EnumQuestionType QuestionType { get; set; }

        public string? MediaPost { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }

        public List<CreateQuestionCommandModel> Questions { get; set; } = new List<CreateQuestionCommandModel>();
    }
}

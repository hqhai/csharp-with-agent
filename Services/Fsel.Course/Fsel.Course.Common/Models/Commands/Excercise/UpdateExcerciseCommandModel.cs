using Fsel.Core.Base.BaseModels;
using Fsel.Course.Common.Models.Commands.Question;
using Fsel.Course.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Common.Models.Commands.Excercise
{
    public class UpdateExcerciseCommandModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }

        public EnumQuestionType QuestionType { get; set; }

        public string? MediaPost { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }

        public List<UpdateQuestionCommandModel> Questions { get; set; } = new List<UpdateQuestionCommandModel>();
    }
}
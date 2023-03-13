using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.EntiyModels
{
    public class ExcerciseModel : BaseEntityModel
    {
        public string? Name { get; set; }

        public EnumQuestionType QuestionType { get; set; }

        public string? MediaPost { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }

        public List<QuestionModel>? Questions { get; set; }
    }
}

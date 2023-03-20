using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class ExerciseModel : BaseEntityModel
    {
        public string? Name { get; set; }

        public EnumQuestionType QuestionType { get; set; }

        public string? MediaPost { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }

        public ICollection<QuestionModel>? Questions { get; set; }
    }
}

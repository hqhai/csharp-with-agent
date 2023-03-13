using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Models.CommandModels.Questions;

namespace Fsel.Course.Domain.Models.CommandModels.Excercises
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

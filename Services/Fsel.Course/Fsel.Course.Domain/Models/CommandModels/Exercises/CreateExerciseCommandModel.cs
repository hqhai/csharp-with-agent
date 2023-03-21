using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Models.CommandModels.Questions;

namespace Fsel.Course.Domain.Models.CommandModels.Exercises
{
    public class CreateExerciseCommandModel
    {
        public string? Name { get; set; }

        public string? MediaPost { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }

        public List<CreateQuestionCommandModel> Questions { get; set; } = new List<CreateQuestionCommandModel>();
    }
}

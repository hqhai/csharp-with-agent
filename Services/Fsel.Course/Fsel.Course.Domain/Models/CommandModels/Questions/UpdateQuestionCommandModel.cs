using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.CommandModels.Questions
{
    public class UpdateQuestionCommandModel
    {
        public Guid Id { get; set; }
        public EnumQuestionType QuestionType { get; set; }
        public bool IsSave { get; set; }
        public object? Config { get; set; }
    }
}

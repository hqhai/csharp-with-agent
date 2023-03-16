namespace Fsel.Course.Domain.Models.EntityModels
{
    public class ExcerciseQuestionModel
    {
        public Guid ExcerciseId { get; set; }

        public Guid QuestionId { get; set; }

        public ICollection<QuestionModel>? Questions { get; set; }
    }
}

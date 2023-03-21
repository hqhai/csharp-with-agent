namespace Fsel.Course.Domain.Models.EntityModels
{
    public class ExerciseQuestionModel
    {
        public Guid ExerciseId { get; set; }

        public Guid QuestionId { get; set; }

        public ICollection<QuestionModel>? Questions { get; set; }
    }
}

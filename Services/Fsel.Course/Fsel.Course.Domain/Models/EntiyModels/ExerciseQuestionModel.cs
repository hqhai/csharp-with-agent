namespace Fsel.Course.Domain.Models.EntiyModels
{
    public class ExerciseQuestionModel
    {
        public Guid ExcerciseId { get; set; }

        public Guid QuestionId { get; set; }

        public List<QuestionModel>? Questions { get; set; }
    }
}

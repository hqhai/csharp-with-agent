namespace Fsel.System.Domain.Models.CommandModels.DailyQuiz
{
    public class DailyQuizCommandModels
    {
        public IList<DailyQuizCommandModel>? DailyQuizzes { get; set; }
    }

    public class DailyQuizCommandModel
    {
        public Guid DailyQuizQuestionId { get; set; }
        public Guid DailyQuizAnswerId { get; set; }
    }
}
